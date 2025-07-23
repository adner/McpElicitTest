# Contact Migration Results

## Summary
- **Total Contacts Processed**: 20
- **Successfully Migrated**: 18
- **Skipped**: 2

## Changes Applied
1. Removed "(sample)" suffix from last names
2. Changed email domain from `example.com` to `nullpointer.se`

## Successfully Migrated Contacts
The following contacts were successfully migrated:

| First Name   | Last Name   | Original Last Name    | New Email                       |
|--------------|-------------|----------------------|--------------------------------|
| Yvonne       | McKay       | McKay (sample)       | someone_a@nullpointer.se       |
| Susanna      | Stubberod   | Stubberod (sample)   | someone_b@nullpointer.se       |
| Nancy        | Anderson    | Anderson (sample)    | someone_c@nullpointer.se       |
| Maria        | Campbell    | Campbell (sample)    | someone_d@nullpointer.se       |
| Sidney       | Higa        | Higa (sample)        | someone_e@nullpointer.se       |
| Scott        | Konersmann  | Konersmann (sample)  | someone_f@nullpointer.se       |
| Robert       | Lyon        | Lyon (sample)        | someone_g@nullpointer.se       |
| Paul         | Cannon      | Cannon (sample)      | someone_h@nullpointer.se       |
| Rene         | Valdes      | Valdes (sample)      | someone_i@nullpointer.se       |
| Jim          | Glynn       | Glynn (sample)       | someone_j@nullpointer.se       |
| Patrick      | Sands       | Sands (sample)       | someone_k@nullpointer.se       |
| Susan        | Burk        | Burk (sample)        | someone_l@nullpointer.se       |
| Thomas       | Andersen    | Andersen (sample)    | someone_m@nullpointer.se       |
| Jim          | Olsson      | Olsson               | No email                       |
| Michael      | Chen        | Chen                 | michael.chen@nullpointer.se    |
| Emma         | Williams    | Williams             | emma.williams@nullpointer.se   |
| James        | Davis       | Davis                | james.davis@nullpointer.se     |
| Kevin        | Garcia      | Garcia               | kevin.garcia@nullpointer.se    |

## Skipped Contacts
The following contacts were skipped during migration:

| Contact ID                            | First Name   | Last Name   |
|---------------------------------------|--------------|-------------|
| 75b99877-4b66-f011-bec2-000d3ab7d535 | Christopher  | Anderson    |
| deb99877-4b66-f011-bec2-000d3ab7d535 | Amanda       | Wilson      |

## Technical Details
- Date of Migration: July 21, 2025
- Target Table: contact
- Domain Replacement: example.com → nullpointer.se
