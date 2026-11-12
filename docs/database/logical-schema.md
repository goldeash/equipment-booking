# Logical schema

The schema `app` has 33 tables in six blocks. The first diagram shows the blocks and how they relate; the second shows every table with its relationships and the columns of the main tables. Both diagrams are Mermaid sources and render on GitHub.

## Blocks

```mermaid
flowchart LR
    subgraph access["Users and access"]
        roles
        departments
        users
        refresh_tokens
    end
    subgraph catalogue["Catalogue"]
        categories
        locations
        features
        equipment_items
        equipment_item_features
        item_conditions
        equipment_condition_changes
    end
    subgraph workflow["Booking workflow"]
        bookings
        booking_statuses
        booking_status_transitions
        cancellation_reasons
        booking_decisions
        decision_sources
        decision_outcomes
        loans
        booking_status_history
    end
    subgraph approval["Approval decision log"]
        approval_assessments
        approval_checks
        approval_check_types
        assessment_outcomes
        purpose_results
        routing_reasons
    end
    subgraph operations["Maintenance and notifications"]
        maintenance_records
        maintenance_types
        notifications
        notification_types
        email_statuses
    end
    subgraph settings["Settings"]
        organisation_settings
        organisation_settings_history
    end

    users -->|requests| bookings
    equipment_items -->|is booked in| bookings
    bookings -->|is assessed by| approval_assessments
    equipment_items -->|is maintained in| maintenance_records
    bookings -->|triggers| notifications
    organisation_settings -.->|threshold and limits recorded in| approval_checks

    classDef accessStyle fill:#e7f5ff,stroke:#1c7ed6,color:#0b3d6b
    classDef catalogueStyle fill:#ebfbee,stroke:#2f9e44,color:#1b4d27
    classDef workflowStyle fill:#fff4e6,stroke:#e8590c,color:#6b2b05
    classDef approvalStyle fill:#f3f0ff,stroke:#7048e8,color:#33206e
    classDef operationsStyle fill:#fff9db,stroke:#f08c00,color:#5c3a00
    classDef settingsStyle fill:#f1f3f5,stroke:#495057,color:#212529
    class roles,departments,users,refresh_tokens accessStyle
    class categories,locations,features,equipment_items,equipment_item_features,item_conditions,equipment_condition_changes catalogueStyle
    class bookings,booking_statuses,booking_status_transitions,cancellation_reasons,booking_decisions,decision_sources,decision_outcomes,loans,booking_status_history workflowStyle
    class approval_assessments,approval_checks,approval_check_types,assessment_outcomes,purpose_results,routing_reasons approvalStyle
    class maintenance_records,maintenance_types,notifications,notification_types,email_statuses operationsStyle
    class organisation_settings,organisation_settings_history settingsStyle
```

| Block | Colour | Purpose |
|---|---|---|
| Users and access | blue | People, roles, departments and refresh tokens |
| Catalogue | green | Items, categories, locations, searchable features and condition history |
| Booking workflow | orange | Requests, allowed status changes, cancellation reasons, decisions, loans and the status history |
| Approval decision log | violet | Result of every policy check with the compared values, the purpose check and the outcome; routing reasons are derived from them |
| Maintenance and notifications | yellow | Maintenance records and in-app notifications (outbox for e-mail) |
| Settings | grey | Organisation wide threshold, automatic approval switch, retention and return time, with every version kept |

## Entity relationship diagram

```mermaid
erDiagram
    roles ||--o{ users : "has"
    departments ||--o{ users : "employs"
    users ||--o{ refresh_tokens : "owns"
    refresh_tokens |o--o| refresh_tokens : "replaced by"
    categories ||--o{ equipment_items : "groups"
    locations ||--o{ equipment_items : "stores"
    item_conditions ||--o{ equipment_items : "condition of"
    equipment_items ||--o{ equipment_item_features : "has"
    features ||--o{ equipment_item_features : "describes"
    equipment_items ||--o{ equipment_condition_changes : "history"
    item_conditions ||--o{ equipment_condition_changes : "from and to"
    users |o--o{ equipment_condition_changes : "changed"
    equipment_items ||--o{ bookings : "is booked in"
    users |o--o{ bookings : "requests"
    booking_statuses ||--o{ bookings : "status of"
    cancellation_reasons |o--o{ bookings : "reason of"
    booking_statuses ||--o{ booking_status_transitions : "from and to"
    bookings ||--o{ booking_decisions : "decided by"
    decision_sources ||--o{ booking_decisions : "source of"
    decision_outcomes ||--o{ booking_decisions : "outcome of"
    users |o--o{ booking_decisions : "decides"
    bookings ||--o| loans : "becomes"
    users ||--o{ loans : "issues and receives"
    item_conditions |o--o{ loans : "returned in"
    bookings ||--o{ booking_status_history : "history"
    booking_statuses ||--o{ booking_status_history : "from and to"
    users |o--o{ booking_status_history : "changed"
    bookings ||--o{ approval_assessments : "assessed by"
    assessment_outcomes ||--o{ approval_assessments : "outcome of"
    purpose_results ||--o{ approval_assessments : "purpose result of"
    approval_assessments ||--|{ approval_checks : "has one per check type"
    approval_check_types ||--o{ approval_checks : "type of"
    routing_reasons ||--o| approval_check_types : "reason when failed"
    equipment_items ||--o{ maintenance_records : "maintained in"
    maintenance_types ||--o{ maintenance_records : "type of"
    users ||--o{ maintenance_records : "records"
    users ||--o{ notifications : "receives"
    notification_types ||--o{ notifications : "type of"
    email_statuses ||--o{ notifications : "e-mail state of"
    bookings |o--o{ notifications : "about"
    users |o--o| organisation_settings : "last changed"
    users |o--o{ organisation_settings_history : "changed"

    users {
        bigint id PK
        citext email UK
        varchar full_name
        varchar password_hash
        varchar role_code FK
        bigint department_id FK
        boolean is_active
    }
    equipment_items {
        bigint id PK
        varchar inventory_number UK
        varchar name
        bigint category_id FK
        bigint location_id FK
        numeric value
        varchar condition_code FK
        boolean is_specialised
        timestamptz retired_at
    }
    categories {
        bigint id PK
        varchar name UK
        int max_loan_days
        varchar usage_guidelines
        boolean is_specialised
        boolean auto_approval_enabled
    }
    bookings {
        bigint id PK
        bigint equipment_item_id FK
        bigint requester_id FK
        date start_date
        date end_date
        varchar purpose
        varchar status_code FK
        varchar cancellation_reason_code FK
        varchar anonymous_ref
    }
    booking_status_transitions {
        varchar from_status_code PK
        varchar to_status_code PK
    }
    booking_decisions {
        bigint id PK
        bigint booking_id FK
        varchar decision_source_code FK
        bigint decided_by_user_id FK
        varchar outcome FK
        timestamptz superseded_at
    }
    loans {
        bigint booking_id PK
        timestamptz issued_at
        timestamptz due_at
        timestamptz returned_at
        varchar return_condition_code FK
    }
    approval_assessments {
        bigint id PK
        bigint booking_id FK
        varchar purpose_result FK
        varchar outcome FK
        timestamptz assessed_at
    }
    approval_checks {
        bigint assessment_id PK
        varchar check_type_code PK
        boolean passed
        numeric observed_value
        numeric limit_value
    }
    approval_check_types {
        varchar code PK
        varchar routing_reason_code FK
    }
    maintenance_records {
        bigint id PK
        bigint equipment_item_id FK
        varchar maintenance_type_code FK
        date start_date
        date end_date
        boolean takes_out_of_service
    }
    organisation_settings_history {
        bigint id PK
        timestamptz changed_at
        bigint changed_by_user_id FK
        numeric low_value_threshold
        boolean auto_approval_enabled
    }
```

Column level details of every table are in [data-dictionary.md](data-dictionary.md); the complete list of foreign keys with their delete rules is in its section Relationships.
