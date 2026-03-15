# Requirements — Mechanics Software

> Derived from the official FIAP Tech Challenge description.
> See [`docs/project/challenge.md`](challenge.md) for the full source.

---

## Functional Requirements (FR)

### Authentication
| ID | Requirement |
|---|---|
| FR-01 | The system must authenticate users via login with email and password |
| FR-02 | The system must issue a JWT token upon successful login |
| FR-03 | All administrative routes must require a valid JWT token |
| FR-04 | The customer status query endpoint must be publicly accessible (no JWT) |

### Customers
| ID | Requirement |
|---|---|
| FR-05 | The system must allow creating, reading, updating, and deleting customers |
| FR-06 | A customer must be identified by CPF (individual) or CNPJ (company) |
| FR-07 | The system must allow searching for a customer by CPF/CNPJ |

### Vehicles
| ID | Requirement |
|---|---|
| FR-08 | The system must allow creating, reading, updating, and deleting vehicles |
| FR-09 | Each vehicle must be linked to an existing customer |
| FR-10 | The system must allow searching for a vehicle by license plate |

### Services Catalogue
| ID | Requirement |
|---|---|
| FR-11 | The system must allow creating, reading, updating, and deleting services |
| FR-12 | Each service must have a name, description, base price, and estimated duration |

### Parts & Inventory
| ID | Requirement |
|---|---|
| FR-13 | The system must allow creating, reading, updating, and deleting parts |
| FR-14 | The system must control stock quantity for each part |
| FR-15 | Every stock change must be recorded as a stock movement (INBOUND, OUTBOUND, RESERVATION, RELEASE) |
| FR-16 | Stock cannot go negative |

### Service Orders
| ID | Requirement |
|---|---|
| FR-17 | The system must allow creating a service order linked to a customer and vehicle |
| FR-18 | The mechanic must start diagnosis before items can be added to the order |
| FR-19 | The system must allow adding services and parts to an order while in `IN_DIAGNOSIS` status |
| FR-20 | When a part with insufficient stock is added, it must be marked as `UNAVAILABLE` and the attendant must be alerted |
| FR-21 | `UNAVAILABLE` parts must be excluded from the budget total |
| FR-22 | The system must automatically generate a budget based on the available items in the order |
| FR-23 | The system must allow sending the budget to the customer (status → `AWAITING_APPROVAL`) |
| FR-24 | The customer must be able to approve or reject the budget |
| FR-25 | Budget approval must transition the order to `IN_EXECUTION` |
| FR-26 | Budget rejection must transition the order to `CANCELLED` and release all stock reservations |
| FR-27 | The system must allow completing and delivering a service order |
| FR-28 | The customer must be able to query the status of their order without authentication |
| FR-29 | The system must provide a metric for average service execution time |

---

## Non-Functional Requirements (NFR)

| ID | Requirement |
|---|---|
| NFR-01 | All admin API routes must be protected with JWT authentication |
| NFR-02 | CPF and CNPJ must be validated using the check digit algorithm |
| NFR-03 | License plates must be validated for both Mercosul (ABC1D23) and legacy (ABC1234) formats |
| NFR-04 | Monetary values must be stored in cents (integer) to avoid floating point errors |
| NFR-05 | APIs must be documented via Swagger |
| NFR-06 | The application must be containerized with a Dockerfile |
| NFR-07 | The full environment must be orchestrated with docker-compose (API + PostgreSQL) |
| NFR-08 | Automated test coverage must reach at least 80% on critical domain flows |
| NFR-09 | The repository must include a README with clear local setup instructions |
| NFR-10 | Passwords must be stored hashed (BCrypt) |
| NFR-11 | API responses must never expose stack traces in production |

---

## Business Rules (BR)

| ID | Rule |
|---|---|
| BR-01 | Items can only be added to a service order with status `IN_DIAGNOSIS` |
| BR-02 | A budget cannot be generated without at least one available service in the order |
| BR-03 | Budget total = sum of prices of `AVAILABLE` items only |
| BR-04 | Service prices are snapshotted at composition time — catalogue changes do not affect existing orders |
| BR-05 | Part prices are snapshotted at composition time |
| BR-06 | Execution cannot start without an approved budget |
| BR-07 | Service order status follows a strict state machine — invalid transitions throw an exception |
| BR-08 | A vehicle must belong to a registered customer |
| BR-09 | Customer document (CPF/CNPJ) must be unique in the system |
| BR-10 | Vehicle license plate must be unique in the system |
| BR-11 | Service name must be unique in the catalogue |

---

## State Machine — Service Order

```
RECEIVED → IN_DIAGNOSIS → AWAITING_APPROVAL → IN_EXECUTION → COMPLETED → DELIVERED
                                            ↘ CANCELLED
```

| Transition | Trigger |
|---|---|
| `RECEIVED` → `IN_DIAGNOSIS` | Mechanic starts diagnosis |
| `IN_DIAGNOSIS` → `AWAITING_APPROVAL` | Budget sent to customer |
| `AWAITING_APPROVAL` → `IN_EXECUTION` | Customer approves budget |
| `AWAITING_APPROVAL` → `CANCELLED` | Customer rejects budget |
| `IN_EXECUTION` → `COMPLETED` | Mechanic completes all services |
| `COMPLETED` → `DELIVERED` | Attendant registers vehicle delivery |
