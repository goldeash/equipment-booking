# Integrity and transactions

Business rules are enforced twice: by the domain model in the API, which gives the user a clear error, and by the database, which is the last line of defence. The database rules hold for concurrent requests, for scripts and for any future client, so invalid data cannot be stored even if the application has a defect. Where a rule exists in both places, an integration test compares them (the blocking statuses of the exclusion constraint and the table of status transitions with the domain model), so a change in one place without the other fails the build.

## Constraints

| Kind | Examples |
|---|---|
| Primary and foreign keys | On every table. Foreign keys use `RESTRICT`, so bookings, items and users with history cannot be deleted; `CASCADE` is used only where the child has no meaning without its parent (feature links of an item, check results of an assessment, sessions and notifications of a user); `SET NULL` keeps history rows when a user reference is removed. The full list is in [data-dictionary.md](data-dictionary.md#relationships) |
| `NOT NULL` and lengths | Every required column; explicit `varchar` limits |
| `UNIQUE` | E-mail, inventory number, names of categories, locations, departments and features, one current decision per booking (partial unique index), one routing reason per check type |
| `CHECK` | Date order, purpose length, value not negative, decline needs a comment, the assistant never declines and is never a person, damaged return needs a comment, a cancelled booking has a cancellation reason and only a cancelled one, only returned loans are anonymised, compared values of a check are given in pairs, singleton settings |
| Reference tables | Every status, outcome and type is a foreign key to a reference table with a readable code; there are no free text or `CHECK` list enumerations |
| Exclusion | `ex_bookings_no_overlap`: `EXCLUDE USING gist (equipment_item_id WITH =, daterange(start_date, end_date, '[]') WITH &&) WHERE (status_code IN ('requested','approved','issued','overdue'))` |

## Booking rules

| Rule | Enforced by |
|---|---|
| Two blocking bookings of an item never overlap, also when they are saved at the same moment | Exclusion constraint `ex_bookings_no_overlap` (SQLSTATE `23P01`) |
| An overdue loan blocks its item until it is returned: no new booking, no change of dates and no issue of another booking. Bookings made before the loan became overdue stay and can still be approved; their issue waits for the return | Trigger `fn_check_item_overdue` (SQLSTATE `EB003`) |
| An out of service maintenance period never overlaps a blocking booking, checked from both sides | Triggers `fn_check_booking_maintenance` on bookings and `fn_check_maintenance_bookings` on maintenance records (SQLSTATE `EB001`) |
| A booking changes its status only along the allowed transitions; a new booking starts as requested | Table `booking_status_transitions` and trigger `fn_check_booking_transition` (SQLSTATE `EB002`) |
| The status matches the loan and the current decision: approved, issued, overdue and returned bookings have a current approval; a declined booking has a current decline; a requested booking has no current decision; issued, overdue and returned bookings have a loan and no other booking has one; a returned loan has a return time | Deferred constraint triggers `fn_check_booking_consistency` on bookings, loans and decisions (SQLSTATE `EB005`), checked at commit when the booking, its decision and its loan are all written |
| Requests not decided by the end of their start date and approved bookings not collected by the end of their period stop blocking the item | Daily job `expired-bookings`; the booking is cancelled with the reason `request_expired` or `not_collected` and the status history records the system as the author |

## Approval route rules

| Rule | Enforced by |
|---|---|
| Every assessment stores one result for each policy check, with the compared values (for example the item value and the low value threshold in force) | Table `approval_checks` with a composite key per check type; deferred trigger `fn_check_assessment_consistency` |
| The outcome follows from the checks: approved only when every check passed and the purpose matched; routed when a check failed or the purpose was not confirmed; pending only while the purpose check runs | Deferred trigger `fn_check_assessment_consistency` (SQLSTATE `EB006`) |
| Routing reasons are derived, not stored a second time | View `v_assessment_routing_reasons`: one reason per failed check (from `approval_check_types`) and one for an unconfirmed or unchecked purpose |
| Every version of the organisation settings is kept | Trigger `fn_log_settings_change` writes `organisation_settings_history` |

## Who may record an action

| Action | Rule | Enforced by |
|---|---|---|
| Decision on a request | Active department manager of the requester's department who is not the requester | Trigger `fn_check_decision_maker` (SQLSTATE `EB004`) and the approval queue, which leaves out the manager's own requests |
| Issue and return | Active administrator | Trigger `fn_check_loan_actors` (SQLSTATE `EB004`) |
| Maintenance record | Active administrator | Trigger `fn_check_maintenance_actor` (SQLSTATE `EB004`) |
| Change of the organisation settings | Active administrator | Trigger `fn_check_settings_actor` (SQLSTATE `EB004`) |

## Triggers and functions

| Object | Purpose |
|---|---|
| `fn_set_updated_at` | Sets `updated_at` on users, items, bookings and settings |
| `fn_acting_user_id` | Reads the user id that the API sets per transaction with `set_config('app.user_id', ..., true)` |
| `fn_log_booking_status` | Writes every status change to `booking_status_history` with the acting user, or none for background jobs |
| `fn_log_condition_change` | Writes every condition change of an item to `equipment_condition_changes` |
| `fn_log_settings_change` | Writes every version of the settings to `organisation_settings_history` |
| `fn_check_booking_transition` | Rejects status changes that are not in `booking_status_transitions` |
| `fn_check_item_overdue` | Keeps an item with an overdue loan blocked until it is returned |
| `fn_check_booking_maintenance`, `fn_check_maintenance_bookings` | Reject overlaps of blocking bookings and out of service maintenance from either side; both lock the item row |
| `fn_check_decision_maker`, `fn_check_loan_actors`, `fn_check_maintenance_actor`, `fn_check_settings_actor`, `fn_ensure_user_role` | Check the role, the activity and the department of the user who records an action |
| `fn_booking_consistency_error`, `fn_check_booking_consistency` | Describe and reject bookings whose status does not match the loan and the current decision |
| `fn_assessment_consistency_error`, `fn_check_assessment_consistency` | Describe and reject assessments whose outcome does not follow from the checks |
| `fn_item_is_available(item, start, end)` | Availability of an item for a date range, including the block by an overdue loan |
| `fn_item_effective_status(item, date)` | Effective status: retired (retirement time set), out_of_service (active maintenance) or available |

## Error codes

Every rule raises its own SQLSTATE; `AppDbContext` translates them in one place into stable API error codes. The API checks the same rules before writing, so these errors normally appear only for writes that bypass the application.

| SQLSTATE | Meaning | API response |
|---|---|---|
| `23P01` (exclusion violation of `ex_bookings_no_overlap`) | Overlapping booking | `409 booking.overlap` with the conflicting periods |
| `EB001` | Booking and out of service maintenance overlap | `409 booking.item_under_maintenance` |
| `EB002` | Status change not allowed | `409 booking.invalid_transition` |
| `EB003` | Item has an overdue loan | `422 booking.item_overdue` |
| `EB004` | User may not record the action | `403 access.forbidden` |
| `EB005` | Booking inconsistent with its loan or decision | `500`; a defect of the application, rejected at commit |
| `EB006` | Assessment outcome inconsistent with its checks | `500`; a defect of the application, rejected at commit |
| `23505` (unique violation) | Duplicate inventory number, e-mail or name | `409 equipment.duplicate_inventory_number`, `409 user.duplicate_email`, `409 reference.duplicate_name` |

## Views

| View | Purpose |
|---|---|
| `v_equipment_with_status` | Items with category, location and effective status today |
| `v_overdue_loans` | Loans not returned after their due time, computed from the due time, so the view is correct even before the overdue job has changed the status |
| `v_requester_return_history` | Loans, late returns and the last late return per requester (shown to managers; anonymised loans excluded) |
| `v_assessment_routing_reasons` | Routing reasons of every assessment, derived from failed checks and the purpose result |
| `v_users_masked` | Users with masked e-mail and name for the read-only role |

## Indexes

| Index | Justification |
|---|---|
| GiST of `ex_bookings_no_overlap` | Overlap checks and availability queries per item and date range |
| `ix_equipment_items_search` (GIN, full text) | Keyword search over name and description |
| `ix_equipment_items_name_trgm` (GIN, trigram) | Partial name search with `ILIKE '%term%'` |
| `ix_loans_open_due_at` (partial, `returned_at IS NULL`) | Daily overdue detection and `v_overdue_loans` scan only open loans |
| `ix_bookings_status_start_date` | Manager queue, expiry job and loans lists filter by status |
| `ix_bookings_requester_id`, `ix_bookings_item_start_date` | My bookings, the booked periods of an item and the overdue check per item |
| `ux_booking_decisions_current` (partial unique) | One current decision per booking and fast lookup |
| `ix_approval_assessments_booking_id`, `ix_approval_assessments_assessed_at` | Latest assessment of a booking and the decision log by time |
| `ix_notifications_user_created_at` | Newest first notification list per user |
| `ix_notifications_email_pending` (partial) | E-mail outbox once a provider is connected |
| Index on every foreign key | Joins and `RESTRICT` checks without sequential scans |

## Transactions

The API uses READ COMMITTED transactions; each use case is one unit of work. Deferred consistency checks run at commit, after all rows of the unit of work are written.

| Use case | Transaction boundary and locking |
|---|---|
| Create a booking | Lock the item row (`SELECT ... FOR UPDATE`), refuse when the item has an overdue loan, insert the booking, run the policy checks, write the assessment, its check results and the decision in one transaction; the exclusion constraint and the triggers reject conflicts |
| Change dates | Lock the item, refuse when the item has an overdue loan, update the dates, supersede the current decision, route again |
| Decide | Refuse an own request, record the decision and change the status in one transaction |
| Issue | Lock the item, refuse when another loan of the item is still issued or overdue, create the loan |
| Mark overdue | The overdue trigger locks the item row, so a new booking and the change to overdue are serialised |
| Record maintenance | Lock the item, list affected bookings, cancel them with the reason `maintenance` only after confirmation, insert the record |
| Retire an item | Lock the item, refuse when current or future blocking bookings exist |
| Expire bookings | One transaction per daily run |
| Anonymisation | Batches of 500 returned loans per transaction |
| Concurrent updates | Optimistic concurrency on `bookings` and `equipment_items` through the PostgreSQL system column `xmin` (API `409 concurrency.conflict`) |
| Transient connection errors | The API retries a unit of work up to five times when the connection drops, for example while the serverless database resumes; every retry repeats the whole transaction |
