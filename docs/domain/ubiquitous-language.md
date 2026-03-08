# Ubiquitous Language — Mechanics Software

## Domain Terms

| Term | Definition |
|---|---|
| **Service Order (OS)** | The central document that records an entire service visit: customer, vehicle, services, parts, budget, and status |
| **Customer** | An individual (CPF) or business (CNPJ) that brings a vehicle for service |
| **Vehicle** | A car identified by license plate, make, model, and year; always linked to a customer |
| **Service** | A technical job to be performed (e.g. oil change, wheel alignment, balancing) |
| **Part** | A physical item or consumable material used during service execution (e.g. filters, oil, brake pads, fluids), subject to inventory control. Consumables are modelled as Parts with a `category` attribute — there is no separate Supply entity. |
| **Budget** | The total cost automatically calculated from services and parts; sent to the customer for approval. Child entity of Service Order — has no independent lifecycle. |
| **Approval** | The customer's formal authorization to proceed with the services |
| **Diagnosis** | The technical evaluation of the vehicle before execution begins |
| **Execution** | The phase where approved services are carried out |
| **Delivery** | The return of the vehicle to the customer after all services are completed |
| **Stock** | The available quantity control for parts |
| **Reservation** | A temporary hold on stock quantity for a specific Service Order |
| **Stock Movement** | A record of every stock change (in or out) for a given part |
| **Service Item** | A service added to a Service Order, with quantity and a price snapshot taken at composition time |
| **Part Item** | A part added to a Service Order, with quantity and a price snapshot taken at composition time |
| **Attendant** | A staff member responsible for creating the OS, registering services, and communicating with the customer |
| **Mechanic** | A technician responsible for performing diagnosis and services |
| **Administrator** | A user with full system access, including CRUDs and reports |

## Service Order Status

| Status | Description | Triggered by |
|---|---|---|
| `RECEIVED` | OS created, vehicle is at the shop | System (on OS creation) |
| `IN_DIAGNOSIS` | Mechanic is evaluating the vehicle | Mechanic |
| `AWAITING_APPROVAL` | Budget generated and sent to customer | System (on budget send) |
| `IN_EXECUTION` | Customer approved; services in progress | System (on approval) |
| `COMPLETED` | All services finished | Mechanic |
| `DELIVERED` | Vehicle returned to the customer | Attendant |
| `CANCELLED` | Customer rejected the budget | System (on rejection) |

## Valid Status Transitions

```
RECEIVED --> IN_DIAGNOSIS
IN_DIAGNOSIS --> AWAITING_APPROVAL
AWAITING_APPROVAL --> IN_EXECUTION     (approval)
AWAITING_APPROVAL --> CANCELLED        (rejection)
IN_EXECUTION --> COMPLETED
COMPLETED --> DELIVERED
```

Any other transition must throw an `InvalidStatusTransitionException`.

## Naming Rules in Code

- Domain classes use the exact terms from this ubiquitous language
- Database table names reflect the terms in `snake_case`
- REST endpoints use the English terms
- Comments and internal docs use the English terms
