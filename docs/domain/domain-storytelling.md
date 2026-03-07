# Domain Storytelling — Mechanics Software

Domain Storytelling captures how the business works through stories told by domain experts. Each story follows the notation:

```
[Actor] → [verb] → [work object] → (to/at/in) [actor or system]
```

Stories are numbered step-by-step and accompanied by a sequence diagram.

---

## Story 1 — Customer Identification

**Scenario:** A customer arrives at the shop. The attendant needs to identify them before opening a service order.

### Steps

```
1. Customer      → arrives at          → Shop (with Vehicle)   → at Attendant
2. Attendant     → searches for        → Customer (by CPF/CNPJ) → in System
3. System        → returns             → Customer record        → to Attendant
   [if not found]
4. Attendant     → registers           → Customer data          → in System
5. System        → validates           → CPF / CNPJ             → (check digit algorithm)
6. System        → saves               → Customer               → in Database
7. System        → confirms            → Customer registration  → to Attendant
```

### Sequence Diagram

```mermaid
sequenceDiagram
    actor Customer
    actor Attendant
    participant System

    Customer->>Attendant: arrives with vehicle
    Attendant->>System: search customer by CPF/CNPJ
    alt customer found
        System-->>Attendant: returns customer record
    else customer not found
        Attendant->>System: register new customer
        System->>System: validate CPF/CNPJ (check digit)
        System-->>Attendant: customer registered
    end
```

---

## Story 2 — Vehicle Identification

**Scenario:** After identifying the customer, the attendant locates or registers the vehicle.

### Steps

```
1. Attendant     → searches for        → Vehicle (by license plate) → in System
2. System        → returns             → Vehicle record              → to Attendant
   [if not found]
3. Attendant     → registers           → Vehicle data                → in System
4. System        → validates           → License plate format        → (Mercosul / legacy)
5. System        → links               → Vehicle                     → to Customer
6. System        → confirms            → Vehicle registration        → to Attendant
```

### Sequence Diagram

```mermaid
sequenceDiagram
    actor Attendant
    participant System

    Attendant->>System: search vehicle by license plate
    alt vehicle found
        System-->>Attendant: returns vehicle record
    else vehicle not found
        Attendant->>System: register new vehicle (plate, make, model, year)
        System->>System: validate license plate format
        System->>System: link vehicle to customer
        System-->>Attendant: vehicle registered
    end
```

---

## Story 3 — Service Order Creation

**Scenario:** With customer and vehicle identified, the attendant opens a service order and adds services and parts.

### Steps

```
1. Attendant     → opens               → Service Order              → in System
2. System        → creates             → Service Order              → with status RECEIVED
3. System        → links               → Customer + Vehicle         → to Service Order
4. Attendant     → adds                → Services (requested)       → to Service Order
5. Attendant     → adds                → Parts / Supplies needed    → to Service Order
6. System        → checks              → Stock availability          → for each Part
7. System        → reserves            → Parts                      → in Stock
8. System        → records             → Stock Movement (RESERVATION) → in Database
```

### Sequence Diagram

```mermaid
sequenceDiagram
    actor Attendant
    participant System
    participant Stock

    Attendant->>System: open service order (customerId, vehicleId)
    System->>System: create ServiceOrder (status: RECEIVED)
    System-->>Attendant: service order created

    loop for each service
        Attendant->>System: add service to order
        System-->>Attendant: service added
    end

    loop for each part
        Attendant->>System: add part to order
        System->>Stock: check availability
        alt stock available
            Stock-->>System: available
            System->>Stock: reserve quantity
            Stock->>Stock: record movement (RESERVATION)
            System-->>Attendant: part added
        else insufficient stock
            Stock-->>System: insufficient
            System-->>Attendant: warning: insufficient stock
        end
    end
```

---

## Story 4 — Diagnosis

**Scenario:** A mechanic receives the vehicle and begins the technical evaluation.

### Steps

```
1. Mechanic      → receives            → Vehicle                    → from Attendant
2. Mechanic      → starts              → Diagnosis                  → in System
3. System        → transitions         → Service Order status        → to IN_DIAGNOSIS
4. Mechanic      → evaluates           → Vehicle                    → physically
5. Mechanic      → adds                → additional Services/Parts   → to Service Order
   (if new issues found during diagnosis)
6. System        → updates             → Service Order items         → in Database
```

### Sequence Diagram

```mermaid
sequenceDiagram
    actor Mechanic
    actor Attendant
    participant System

    Attendant->>Mechanic: hands over vehicle
    Mechanic->>System: start diagnosis (serviceOrderId)
    System->>System: validate transition RECEIVED → IN_DIAGNOSIS
    System-->>Mechanic: status updated to IN_DIAGNOSIS

    opt new issues found
        Mechanic->>System: add more services / parts
        System-->>Mechanic: items updated
    end
```

---

## Story 5 — Budget Generation and Approval

**Scenario:** After diagnosis, the system calculates the budget and sends it to the customer for approval.

### Steps

```
1. Attendant     → requests            → Budget generation          → from System
2. System        → calculates          → Budget                     → from services + parts prices
3. System        → creates             → Budget record              → in Database
4. Attendant     → sends               → Budget                     → to Customer (via API)
5. System        → transitions         → Service Order status        → to AWAITING_APPROVAL
6. Customer      → reviews             → Budget                     → via API
   [if approved]
7. Customer      → approves            → Budget                     → via API
8. System        → transitions         → Service Order status        → to IN_EXECUTION
   [if rejected]
7. Customer      → rejects             → Budget                     → via API
8. System        → transitions         → Service Order status        → to CANCELLED
9. System        → releases            → Part reservations          → in Stock
10. System       → records             → Stock Movement (RELEASE)   → in Database
```

### Sequence Diagram

```mermaid
sequenceDiagram
    actor Attendant
    actor Customer
    participant System
    participant Stock

    Attendant->>System: generate budget (serviceOrderId)
    System->>System: calculate total (services + parts)
    System->>System: create Budget record
    System-->>Attendant: budget generated

    Attendant->>System: send budget to customer
    System->>System: status → AWAITING_APPROVAL
    System-->>Customer: budget notification

    alt customer approves
        Customer->>System: approve budget (serviceOrderId)
        System->>System: status → IN_EXECUTION
        System-->>Customer: approval confirmed
    else customer rejects
        Customer->>System: reject budget (serviceOrderId)
        System->>System: status → CANCELLED
        System->>Stock: release all part reservations
        Stock->>Stock: record movements (RELEASE)
        System-->>Customer: cancellation confirmed
    end
```

---

## Story 6 — Service Execution

**Scenario:** With the budget approved, the mechanic executes the services and confirms part usage.

### Steps

```
1. Mechanic      → receives            → Service Order (approved)   → from System
2. Mechanic      → executes            → Services                   → on Vehicle
3. Mechanic      → uses                → Parts                      → during execution
4. System        → deducts             → Parts                      → from Stock
5. System        → records             → Stock Movement (OUTBOUND)  → in Database
6. Mechanic      → completes           → Service Order              → in System
7. System        → transitions         → Service Order status        → to COMPLETED
```

### Sequence Diagram

```mermaid
sequenceDiagram
    actor Mechanic
    participant System
    participant Stock

    Mechanic->>System: start execution (serviceOrderId)
    System->>System: confirm status is IN_EXECUTION

    loop for each service
        Mechanic->>System: execute service
        System-->>Mechanic: service marked as done
    end

    loop for each part used
        Mechanic->>System: confirm part usage
        System->>Stock: deduct quantity from stock
        Stock->>Stock: record movement (OUTBOUND)
        System-->>Mechanic: part usage confirmed
    end

    Mechanic->>System: complete service order
    System->>System: status → COMPLETED
    System-->>Mechanic: order completed
```

---

## Story 7 — Vehicle Delivery

**Scenario:** The service is complete. The attendant registers the vehicle delivery to the customer.

### Steps

```
1. Customer      → comes to            → Shop                       → to pick up Vehicle
2. Attendant     → verifies            → Service Order              → in System
3. Attendant     → registers           → Vehicle delivery           → in System
4. System        → transitions         → Service Order status        → to DELIVERED
5. System        → records             → Delivery timestamp          → in Database
6. Attendant     → returns             → Vehicle                    → to Customer
```

### Sequence Diagram

```mermaid
sequenceDiagram
    actor Customer
    actor Attendant
    participant System

    Customer->>Attendant: arrives to pick up vehicle
    Attendant->>System: get service order details (serviceOrderId)
    System-->>Attendant: service order (status: COMPLETED)
    Attendant->>System: register delivery (serviceOrderId)
    System->>System: validate transition COMPLETED → DELIVERED
    System->>System: record delivery timestamp
    System-->>Attendant: delivery registered
    Attendant->>Customer: returns vehicle
```

---

## Story 8 — Customer Status Query (Public)

**Scenario:** A customer wants to check the progress of their vehicle repair without logging in.

### Steps

```
1. Customer      → queries             → Service Order status        → via API (no login)
2. System        → retrieves           → Service Order               → from Database
3. System        → returns             → Current status + summary    → to Customer
```

### Sequence Diagram

```mermaid
sequenceDiagram
    actor Customer
    participant API
    participant System

    Customer->>API: GET /api/service-orders/{id}/status
    Note over API: No JWT required
    API->>System: get service order status
    System-->>API: status + summary
    API-->>Customer: current status (e.g. IN_EXECUTION)
```

---

## Story 9 — Inventory Management

**Scenario:** An administrator manages the parts catalog and replenishes stock.

### Steps

```
1. Admin         → registers           → Part (code, name, price)   → in System
2. System        → validates           → Part data                  → (unique code, positive price)
3. System        → saves               → Part                       → in Database
4. Admin         → replenishes         → Stock                      → for Part
5. System        → updates             → Stock quantity              → for Part
6. System        → records             → Stock Movement (INBOUND)   → in Database
```

### Sequence Diagram

```mermaid
sequenceDiagram
    actor Admin
    participant System

    Admin->>System: register part (code, name, price, initial stock)
    System->>System: validate part data
    System-->>Admin: part registered

    Admin->>System: replenish stock (partId, quantity)
    System->>System: update stock quantity
    System->>System: record movement (INBOUND)
    System-->>Admin: stock updated
```

---

## Actor Summary

| Actor | Role | Key Interactions |
|---|---|---|
| **Customer** | Vehicle owner | Arrives, approves/rejects budget, queries status, picks up vehicle |
| **Attendant** | Front-desk staff | Identifies customer/vehicle, creates OS, adds items, sends budget, registers delivery |
| **Mechanic** | Shop technician | Starts diagnosis, executes services, confirms part usage, completes OS |
| **Administrator** | System manager | Manages parts catalog, replenishes stock, manages users |
| **System** | Internal automations | Validates data, transitions status, calculates budget, manages stock movements |

---

## Work Objects Summary

| Work Object | Description |
|---|---|
| **Customer** | Person identified by CPF/CNPJ |
| **Vehicle** | Car identified by license plate |
| **Service Order** | Central document linking customer, vehicle, services, parts, and budget |
| **Service** | Technical job to be performed |
| **Part / Supply** | Physical item with stock control |
| **Budget** | Calculated cost sent for customer approval |
| **Stock Movement** | Record of every stock change (inbound, outbound, reservation, release) |
