# Event Storming — Mechanics Software

## Legend

| Symbol | Type | Description |
|---|---|---|
| `[EVT]` | Domain Event | Something that happened in the system (orange) |
| `[CMD]` | Command | An intent/action that triggers an event (blue) |
| `[AGG]` | Aggregate | The entity that handles the command (yellow) |
| `[POL]` | Policy / Rule | An automatic reaction to an event (purple) |
| `[ACT]` | Actor | Who triggers the command (beige) |
| `[HOT]` | Hot Spot | An open question or problem (red) |

---

## Flow 1 — Service Order Creation and Tracking

### Step 1 — Customer Identification

```
[ACT] Customer
  |
  v
[CMD] Identify customer by CPF/CNPJ
  |
  v
[AGG] Customer
  |
  +-- found     --> [EVT] Customer Identified
  |
  +-- not found --> [CMD] Register Customer
                         |
                         v
                    [POL] CPF/CNPJ must be valid (check digit algorithm)
                    [POL] Email must have a valid format
                         |
                         v
                    [EVT] Customer Registered
```

---

### Step 2 — Vehicle Identification

```
[ACT] Attendant
  |
  v
[CMD] Locate vehicle by license plate
  |
  v
[AGG] Vehicle
  |
  +-- found     --> [EVT] Vehicle Located
  |
  +-- not found --> [CMD] Register Vehicle
                         |
                         v
                    [POL] License plate must be valid (Mercosul ABC1D23 or legacy ABC-1234)
                    [POL] Vehicle must be linked to an existing customer
                         |
                         v
                    [EVT] Vehicle Registered
```

---

### Step 3 — Service Order Opening

```
[ACT] Attendant
  |
  v
[CMD] Open Service Order
  |
  v
[AGG] ServiceOrder
  |
  v
[POL] Initial status must be RECEIVED
[POL] OS must reference a valid customer and vehicle
  |
  v
[EVT] Service Order Created (status: RECEIVED)
```

---

### Step 4 — Service Order Composition

```
[ACT] Attendant / Mechanic
  |
  +-- [CMD] Add Service to OS
  |         |
  |         v
  |    [AGG] ServiceOrder
  |         |
  |         v
  |    [POL] OS must be in RECEIVED or IN_DIAGNOSIS status
  |         |
  |         v
  |    [EVT] Service Added to OS
  |
  +-- [CMD] Add Part to OS
            |
            v
       [AGG] ServiceOrder
            |
            v
       [POL] OS must be in RECEIVED or IN_DIAGNOSIS status
       [POL] Check stock availability before adding
            |
            +-- available     --> [EVT] Part Added to OS
            |                    [EVT] Part Reserved in Stock
            |
            +-- not available --> [HOT] Insufficient stock
                                   Block the action or warn the attendant?
```

---

### Step 5 — Budget Generation and Sending

```
[ACT] System / Attendant
  |
  v
[CMD] Generate Budget
  |
  v
[AGG] Budget
  |
  v
[POL] Total = sum(service price * qty) + sum(part price * qty)
[POL] OS must have at least one service
  |
  v
[EVT] Budget Generated

  |
  v
[CMD] Send Budget to Customer
  |
  v
[AGG] ServiceOrder
  |
  v
[POL] Status automatically changes to AWAITING_APPROVAL
  |
  v
[EVT] Budget Sent to Customer
[EVT] Service Order Status Updated to AWAITING_APPROVAL
```

---

### Step 6 — Approval or Rejection

```
[ACT] Customer
  |
  +-- [CMD] Approve Budget
  |         |
  |         v
  |    [AGG] ServiceOrder
  |         |
  |         v
  |    [POL] Status changes to IN_EXECUTION
  |         |
  |         v
  |    [EVT] Budget Approved
  |    [EVT] Service Order Status Updated to IN_EXECUTION
  |
  +-- [CMD] Reject Budget
            |
            v
       [AGG] ServiceOrder
            |
            v
       [POL] Status changes to CANCELLED
       [POL] All stock reservations must be released
            |
            v
       [EVT] Budget Rejected
       [EVT] Service Order Status Updated to CANCELLED
       [EVT] Part Reservations Released
```

---

### Step 7 — Diagnosis

```
[ACT] Mechanic
  |
  v
[CMD] Start Diagnosis
  |
  v
[AGG] ServiceOrder
  |
  v
[POL] Valid transition: RECEIVED --> IN_DIAGNOSIS
  |
  v
[EVT] Diagnosis Started
[EVT] Service Order Status Updated to IN_DIAGNOSIS

[HOT] Diagnosis may uncover new services or parts.
      This requires returning to Step 4 before generating the budget.
```

---

### Step 8 — Execution

```
[ACT] Mechanic
  |
  v
[CMD] Execute Services
  |
  v
[AGG] ServiceOrder
  |
  v
[POL] OS must be in IN_EXECUTION status
[POL] When a part is used, deduct from stock
  |
  v
[EVT] Service Executed
[EVT] Part Used in OS
[EVT] Part Deducted from Stock
[EVT] Stock Movement Recorded
```

---

### Step 9 — Completion and Delivery

```
[ACT] Mechanic
  |
  v
[CMD] Complete Service Order
  |
  v
[AGG] ServiceOrder
  |
  v
[POL] Valid transition: IN_EXECUTION --> COMPLETED
  |
  v
[EVT] Service Order Completed
[EVT] Service Order Status Updated to COMPLETED

  |
  v
[ACT] Attendant
  |
  v
[CMD] Register Vehicle Delivery
  |
  v
[AGG] ServiceOrder
  |
  v
[POL] Valid transition: COMPLETED --> DELIVERED
  |
  v
[EVT] Vehicle Delivered to Customer
[EVT] Service Order Status Updated to DELIVERED
```

---

## Flow 2 — Parts and Inventory Management

### Part Registration

```
[ACT] Administrator
  |
  v
[CMD] Register Part
  |
  v
[AGG] Part
  |
  v
[POL] Part code must be unique
[POL] Unit price must be positive
[POL] Initial stock quantity >= 0
  |
  v
[EVT] Part Registered

  |
  v
[CMD] Update Part
  |
  v
[EVT] Part Updated
```

---

### Stock Movements

```
[ACT] Administrator / System
  |
  +-- [CMD] Replenish Stock
  |         |
  |         v
  |    [AGG] Part
  |         |
  |         v
  |    [POL] Quantity must be positive
  |         |
  |         v
  |    [EVT] Stock Replenished
  |    [EVT] Stock Movement Recorded (type: INBOUND)
  |
  +-- [CMD] Reserve Part for OS
  |         |
  |         v
  |    [AGG] Part
  |         |
  |         v
  |    [POL] Available quantity must be >= requested quantity
  |         |
  |         +-- ok          --> [EVT] Part Reserved for OS
  |         |                   [EVT] Stock Movement Recorded (type: RESERVATION)
  |         |
  |         +-- insufficient --> [EVT] Insufficient Stock Identified
  |                              [HOT] Block addition to OS or allow with warning?
  |
  +-- [CMD] Confirm Part Usage (after execution)
  |         |
  |         v
  |    [AGG] Part
  |         |
  |         v
  |    [POL] Reservation must exist for the OS
  |    [POL] Stock cannot go negative
  |         |
  |         v
  |    [EVT] Part Usage Confirmed
  |    [EVT] Part Deducted from Stock
  |    [EVT] Stock Movement Recorded (type: OUTBOUND)
  |
  +-- [CMD] Release Part Reservation (when OS is cancelled)
            |
            v
       [AGG] Part
            |
            v
       [POL] Reservation must exist for the OS
            |
            v
       [EVT] Part Reservation Released
       [EVT] Stock Movement Recorded (type: RELEASE)
```

---

## State Machine — Service Order Status

```
              RECEIVED
                 |
                 v
           IN_DIAGNOSIS
                 |
                 v
        AWAITING_APPROVAL
             /         \
  (approve) /           \ (reject)
           v             v
      IN_EXECUTION    CANCELLED
           |
           v
        COMPLETED
           |
           v
        DELIVERED
```

### Valid Transitions

| From | To | Trigger |
|---|---|---|
| `RECEIVED` | `IN_DIAGNOSIS` | Mechanic starts diagnosis |
| `IN_DIAGNOSIS` | `AWAITING_APPROVAL` | Budget sent to customer |
| `AWAITING_APPROVAL` | `IN_EXECUTION` | Customer approves budget |
| `AWAITING_APPROVAL` | `CANCELLED` | Customer rejects budget |
| `IN_EXECUTION` | `COMPLETED` | Mechanic completes all services |
| `COMPLETED` | `DELIVERED` | Attendant registers vehicle pickup |

Any other transition must throw `InvalidStatusTransitionException`.

---

## Hot Spots (Open Questions)

| # | Description | Impact |
|---|---|---|
| 1 | Diagnosis may uncover new services — how to reopen OS composition? | Medium |
| 2 | Insufficient stock when adding a part — block or warn? | High |
| 3 | Customer queries status without authentication — how to identify them? | Medium |
| 4 | Partial item cancellation on an OS before approval | Low |

---

## System Actors

| Actor | Description | Permissions |
|---|---|---|
| **Customer** | Vehicle owner | Query OS status (public), approve/reject budget |
| **Attendant** | Front-desk staff | Create OS, register items, send budget, register delivery |
| **Mechanic** | Shop technician | Start diagnosis, start execution, complete OS |
| **Administrator** | System manager | Full access: CRUDs, reports, users |
| **System** | Internal automations | Generate budget, update status, record stock movements |
