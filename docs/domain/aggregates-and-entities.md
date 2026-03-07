# Aggregates, Entities and Value Objects

## Entity Relationship Diagram

```mermaid
erDiagram
    Customer {
        uuid id
        string person_type
        string document
        string name
        string email
        string phone
    }
    Vehicle {
        uuid id
        string license_plate
        string make
        string model
        int year
        uuid customer_id
    }
    ServiceOrder {
        uuid id
        uuid customer_id
        uuid vehicle_id
        string status
        decimal total
        datetime created_at
        datetime completed_at
    }
    Service {
        uuid id
        string name
        string description
        decimal base_price
        int estimated_minutes
    }
    Part {
        uuid id
        string code
        string name
        decimal unit_price
        int stock_quantity
    }
    Budget {
        uuid id
        uuid service_order_id
        decimal services_total
        decimal parts_total
        decimal total
        string status
        datetime generated_at
    }
    ServiceItem {
        uuid id
        uuid service_order_id
        uuid service_id
        int quantity
        decimal unit_price
    }
    PartItem {
        uuid id
        uuid service_order_id
        uuid part_id
        int quantity
        decimal unit_price
    }
    StockMovement {
        uuid id
        uuid part_id
        string type
        int quantity
        string reference
        datetime created_at
    }

    Customer ||--o{ Vehicle : owns
    Customer ||--o{ ServiceOrder : requests
    Vehicle ||--o{ ServiceOrder : subject-of
    ServiceOrder ||--o{ ServiceItem : contains
    ServiceOrder ||--o{ PartItem : contains
    ServiceOrder ||--|| Budget : has
    ServiceItem }o--|| Service : references
    PartItem }o--|| Part : references
    Part ||--o{ StockMovement : tracks
```

## Aggregate Roots

### ServiceOrder (Main Aggregate Root)

The most complex aggregate in the system. Encapsulates all business rules for the OS lifecycle.

**Responsibilities:**
- Manage addition of services and parts
- Calculate and generate the budget
- Control status transitions (state machine)
- Enforce business invariants

**Child entities:**
- `ServiceItem` — a service added to the OS
- `PartItem` — a part added to the OS
- `Budget` — the calculated value sent for approval

**Invariant rules:**
- Cannot add items to an OS with status other than `RECEIVED` or `IN_DIAGNOSIS`
- Cannot start execution without an approved budget
- Status can only advance according to the defined state machine

---

### Customer (Aggregate Root)

**Attributes:**
- `id` — UUID
- `personType` — `INDIVIDUAL` | `COMPANY`
- `document` — Value Object `TaxId` (CPF or CNPJ)
- `name` — string
- `email` — Value Object `Email`
- `phone` — string

**Rules:**
- `document` must be unique per customer
- `personType` determines the document validation format

---

### Vehicle (Aggregate Root)

**Attributes:**
- `id` — UUID
- `licensePlate` — Value Object `LicensePlate`
- `make` — string
- `model` — string
- `year` — number
- `customerId` — reference to Customer

**Rules:**
- `licensePlate` must be unique in the system
- `year` cannot be greater than current year + 1

---

### Part (Aggregate Root — Inventory context)

**Attributes:**
- `id` — UUID
- `code` — unique string
- `name` — string
- `description` — string
- `unitPrice` — Value Object `Money`
- `stockQuantity` — number

**Child entities:**
- `StockMovement`

**Rules:**
- Stock cannot go negative
- Every quantity change generates a `StockMovement`

---

### Service (Aggregate Root — catalog)

**Attributes:**
- `id` — UUID
- `name` — string
- `description` — string
- `basePrice` — Value Object `Money`
- `estimatedMinutes` — number

---

## Value Objects

### TaxId

```
type: 'CPF' | 'CNPJ'
value: string (digits only)

Validations:
- CPF:  11 digits, check digit algorithm
- CNPJ: 14 digits, check digit algorithm
```

### LicensePlate

```
value: string

Accepted formats:
- Mercosul: ABC1D23  (3 letters, 1 digit, 1 letter, 2 digits)
- Legacy:   ABC1234  (3 letters, 4 digits)
```

### Money

```
amount: number (in cents — integer)
currency: 'BRL'

Operations:
- add(other: Money): Money
- multiply(factor: number): Money
- toFormatted(): string   ("R$ 150,00")
```

Using cents avoids floating point errors.

### Email

```
value: string
Validation: simplified RFC 5322 format
```

### ServiceOrderStatus

```
Enum with valid transitions encapsulated.
Throws DomainException for invalid transitions.
```

---

## Entities (non-Aggregate Root)

### ServiceItem
```
id
serviceOrderId
serviceId
quantity
unitPrice  (price snapshot at the time of addition)
```

### PartItem
```
id
serviceOrderId
partId
quantity
unitPrice  (price snapshot at the time of addition)
```

### Budget
```
id
serviceOrderId
servicesTotal: Money
partsTotal:    Money
total:         Money
status:        'PENDING' | 'APPROVED' | 'REJECTED'
generatedAt:   Date
```

### StockMovement
```
id
partId
type:      'INBOUND' | 'OUTBOUND' | 'RESERVATION' | 'RELEASE'
quantity:  number
reference: string  (e.g. service order number)
createdAt: Date
```
