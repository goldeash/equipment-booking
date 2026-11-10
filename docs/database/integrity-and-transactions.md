# Integrity and transactions

Business rules are enforced twice: by the domain model in the API and by the database. The database is the last line of defence, so the rules hold even for concurrent requests and for data loaded by scripts.

## Constraints

| Kind | Examples |
|---|---|
| Primary and foreign keys | On every table; foreign keys use `RESTRICT` for business data and `CASCADE` only for owned children (decisions, loan, assessment reasons) |
| `NOT NULL` and lengths | Every required column; explicit `varchar` limits |
| `UNIQUE` | E-mail, inventory number, names of categories, locations, departments and features, one current decision per booking (partial unique index) |
| `CHECK` | Date order, purpose length, value not negative, outcome and status lists, decline needs a comment, the assistant never declines, damaged return needs a comment, anonymisation consistency, singleton settings |
| Exclusion | `ex_bookings_no_overlap`: `EXCLUDE USING gist (equipment_item_id WITH =, daterange(start_date, end_date, '[]') WITH &&) WHERE (status_code IN ('requested','approved','issued','overdue'))` |

The exclusion constraint implements the booking rule: a booking blocks its item from creation until it is declined, cancelled or returned, and an overdue loan keeps blocking. Two requests that overlap cannot both be saved, even when they arrive at the same moment; the second insert fails with SQLSTATE `23P01`, which the API returns as `409 booking.overlap` with the conflicting periods.

## Triggers and functions

| Object | Purpose |
|---|---|
| `fn_set_updated_at` | Sets `updated_at` on users, items, bookings and settings |
| `fn_log_booking_status` | Writes every status change to `booking_status_history` with the acting user |
| `fn_log_condition_change` | Writes every condition change of an item to `equipment_condition_changes` |
| `fn_check_booking_maintenance` | Locks the item row and rejects a blocking booking that overlaps an out of service maintenance period (SQLSTATE `EB001`, API `409 booking.item_under_maintenance`) |
| `fn_acting_user_id` | Reads the user id that the API sets per transaction with `set_config('app.user_id', ..., true)` |
| `fn_item_is_available(item, start, end)` | Availability of an item for a date range |
| `fn_item_effective_status(item, date)` | Effective status: retired, out_of_service or available |

## Views

| View | Purpose |
|---|---|
| `v_equipment_with_status` | Items with category, location and effective status today |
| `v_overdue_loans` | Overdue loans and how long they are overdue |
| `v_requester_return_history` | Loans, late returns and the last late return per requester (shown to managers; anonymised loans excluded) |
| `v_users_masked` | Users with masked e-mail and name for the read-only role |

## Indexes

| Index | Justification |
|---|---|
| GiST of `ex_bookings_no_overlap` | Overlap checks and availability queries per item and date range |
| `ix_equipment_items_search` (GIN, full text) | Keyword search over name and description |
| `ix_equipment_items_name_trgm` (GIN, trigram) | Partial name search with `ILIKE '%term%'` |
| `ix_loans_open_due_at` (partial, `returned_at IS NULL`) | Daily overdue detection scans only open loans |
| `ix_bookings_status_start_date` | Manager queue and loans lists filter by status |
| `ix_bookings_requester_id`, `ix_bookings_item_start_date` | My bookings and the booked periods of an item |
| `ux_booking_decisions_current` (partial unique) | One current decision per booking and fast lookup |
| `ix_notifications_user_created_at` | Newest first notification list per user |
| `ix_notifications_email_pending` (partial) | E-mail outbox once a provider is connected |
| Indexes on every foreign key | Joins and `RESTRICT` checks without sequential scans |

## Transactions

The API uses READ COMMITTED transactions; each use case is one unit of work.

| Use case | Transaction boundary and locking |
|---|---|
| Create a booking | Lock the item row (`SELECT ... FOR UPDATE`), insert the booking, run the policy checks, write the assessment and the decision in one transaction; the exclusion constraint and the maintenance trigger reject conflicts |
| Change dates | Lock the item, update the dates, supersede the current decision, route again |
| Issue | Lock the item, refuse when another loan of the item is still issued or overdue, create the loan |
| Record maintenance | Lock the item, list affected bookings, cancel them only after confirmation, insert the record |
| Retire an item | Lock the item, refuse when current or future blocking bookings exist |
| Anonymisation | Batches of 500 returned loans per transaction |
| Concurrent updates | Optimistic concurrency on `bookings` and `equipment_items` through the PostgreSQL system column `xmin` (API `409 concurrency.conflict`) |

Database errors are translated in one place (`AppDbContext.SaveChangesAsync`) into stable API error codes.
