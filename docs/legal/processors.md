# Personal data and processors

The system processes the names, work e-mail addresses, departments and roles of employees, their bookings, loans and returns, condition notes and the log of decisions on requests. The legal basis is the legitimate interest of the organisation in managing its property (Article 6(1)(f) GDPR). The privacy notice shown at the first sign in is part of the application (`frontend/src/pages/privacy/PrivacyNoticeText.tsx`).

## Processors

| Processor | Purpose | Data | Status |
|---|---|---|---|
| Neon (PostgreSQL hosting, AWS region Europe Central 1, Frankfurt) | Storage of all application data | All personal data listed above | In use; the deploying organisation concludes a data processing agreement with the provider |
| Application hosting provider | Running the API and the web application | Data in transit, application logs without personal data | Chosen at deployment |
| E-mail service | Delivery of notification e-mails | Recipient address, notification text | Not connected; the outbox exists but no e-mail is sent |
| Language model provider | Purpose check and search of the AI assistant | Request purpose text and catalogue data only, never the identity or history of the requester | Not connected; without it every request is routed to a manager |

## Measures

- Access is limited by role in the API and by database roles (`docs/database/roles-and-access.md`).
- Passwords are stored only as salted PBKDF2 hashes; refresh tokens only as SHA-256 hashes.
- Logs contain no names, e-mail addresses, purposes or tokens.
- Personal data of returned loans are anonymised after the retention period set by the organisation (default 365 days).
- Before the system is put into service, the deploying organisation carries out a data protection impact assessment (Article 35 GDPR) and a balancing test of the legitimate interest.
