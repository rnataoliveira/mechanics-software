# Project Roadmap — Delivery Priority

Issues ordered by execution dependency. Each issue should be started only after its predecessors are merged.

**Deadline:** May 1, 2026
**Repository:** https://github.com/rnataoliveira/mechanics-software
**Project Board:** https://github.com/users/rnataoliveira/projects/2

---

## P1 — Setup (unblocks everything)

| Priority | Issue | Title | Milestone |
|---|---|---|---|
| 1 | [#45](https://github.com/rnataoliveira/mechanics-software/issues/45) | Scaffold .NET solution structure | M1 |

> Nothing else can start until the solution structure exists and `dotnet build` passes.

---

## P2 — Domain Layer (M1 · due Mar 22)

Order matters: shared bases → value objects → entities → aggregates.

| Priority | Issue | Title | Depends on |
|---|---|---|---|
| 2 | [#9](https://github.com/rnataoliveira/mechanics-software/issues/9) | Shared base classes (Entity, ValueObject, DomainException, Money) | #45 |
| 3 | [#10](https://github.com/rnataoliveira/mechanics-software/issues/10) | TaxId value object | #9 |
| 3 | [#11](https://github.com/rnataoliveira/mechanics-software/issues/11) | LicensePlate value object | #9 |
| 3 | [#12](https://github.com/rnataoliveira/mechanics-software/issues/12) | Email value object | #9 |
| 3 | [#17](https://github.com/rnataoliveira/mechanics-software/issues/17) | ServiceOrderStatus value object | #9 |
| 3 | [#18](https://github.com/rnataoliveira/mechanics-software/issues/18) | BudgetStatus value object | #9 |
| 4 | [#13](https://github.com/rnataoliveira/mechanics-software/issues/13) | Customer aggregate | #10, #12 |
| 4 | [#14](https://github.com/rnataoliveira/mechanics-software/issues/14) | Vehicle aggregate | #11 |
| 4 | [#15](https://github.com/rnataoliveira/mechanics-software/issues/15) | Service aggregate | #9 |
| 4 | [#16](https://github.com/rnataoliveira/mechanics-software/issues/16) | Part aggregate + StockMovement entity | #9 |
| 5 | [#19](https://github.com/rnataoliveira/mechanics-software/issues/19) | ServiceOrder aggregate root | #17, #18, #13, #14, #15, #16 |

---

## P3 — Application Layer (M2 · due Apr 5)

| Priority | Issue | Title | Depends on |
|---|---|---|---|
| 6 | [#20](https://github.com/rnataoliveira/mechanics-software/issues/20) | IAppDbContext interface + NotFoundException | #19 |
| 7 | [#21](https://github.com/rnataoliveira/mechanics-software/issues/21) | Auth — LoginUseCase | #20 |
| 7 | [#22](https://github.com/rnataoliveira/mechanics-software/issues/22) | Customers — CRUD use cases | #20 |
| 7 | [#23](https://github.com/rnataoliveira/mechanics-software/issues/23) | Vehicles — CRUD use cases | #20 |
| 7 | [#24](https://github.com/rnataoliveira/mechanics-software/issues/24) | Services Catalogue — CRUD use cases | #20 |
| 7 | [#25](https://github.com/rnataoliveira/mechanics-software/issues/25) | Inventory — CRUD + stock use cases | #20 |
| 8 | [#26](https://github.com/rnataoliveira/mechanics-software/issues/26) | Service Order — lifecycle use cases (15 use cases) | #21, #22, #23, #24, #25 |

> Issues #21–#25 can be developed in parallel after #20 is merged.

---

## P4 — Infrastructure Layer (M3 · due Apr 12)

| Priority | Issue | Title | Depends on |
|---|---|---|---|
| 9 | [#46](https://github.com/rnataoliveira/mechanics-software/issues/46) | User entity + database seed | #20 |
| 9 | [#29](https://github.com/rnataoliveira/mechanics-software/issues/29) | JwtProvider | #20 |
| 9 | [#30](https://github.com/rnataoliveira/mechanics-software/issues/30) | PasswordHasher (BCrypt) | #20 |
| 10 | [#27](https://github.com/rnataoliveira/mechanics-software/issues/27) | AppDbContext + EF Core Fluent API configurations | #46 |
| 11 | [#28](https://github.com/rnataoliveira/mechanics-software/issues/28) | Initial EF Core migration | #27 |

> #29 and #30 can be done in parallel with #46 and #27.

---

## P5 — API Layer (M4 · due Apr 19)

| Priority | Issue | Title | Depends on |
|---|---|---|---|
| 12 | [#34](https://github.com/rnataoliveira/mechanics-software/issues/34) | DI registration + appsettings configuration | #28 |
| 13 | [#33](https://github.com/rnataoliveira/mechanics-software/issues/33) | JWT middleware + global exception handling | #34 |
| 13 | [#32](https://github.com/rnataoliveira/mechanics-software/issues/32) | Swagger configuration | #34 |
| 14 | [#31](https://github.com/rnataoliveira/mechanics-software/issues/31) | All controllers (Auth, Customers, Vehicles, Services, Parts, ServiceOrders) | #33, #32 |

---

## P6 — Tests (M5 · due Apr 26)

Unit tests can start as soon as the domain is implemented. Integration tests require the full stack.

| Priority | Issue | Title | Depends on |
|---|---|---|---|
| 15 | [#35](https://github.com/rnataoliveira/mechanics-software/issues/35) | Unit tests — value objects | #10, #11, #12, #9 |
| 15 | [#36](https://github.com/rnataoliveira/mechanics-software/issues/36) | Unit tests — ServiceOrder state machine and business rules | #19 |
| 16 | [#37](https://github.com/rnataoliveira/mechanics-software/issues/37) | Unit tests — application use cases (mocked) | #26 |
| 17 | [#38](https://github.com/rnataoliveira/mechanics-software/issues/38) | Integration tests — Customers and Inventory endpoints | #31 |
| 17 | [#39](https://github.com/rnataoliveira/mechanics-software/issues/39) | Integration test — full Service Order flow | #31 |
| 18 | [#40](https://github.com/rnataoliveira/mechanics-software/issues/40) | Verify 80%+ code coverage | #35, #36, #37 |

> #35 and #36 can start during M1 as soon as the domain classes are merged.
> #38 and #39 can run in parallel.

---

## P7 — Final Deliverables (M6 · due May 1)

| Priority | Issue | Title | Depends on |
|---|---|---|---|
| 19 | [#41](https://github.com/rnataoliveira/mechanics-software/issues/41) | Dockerfile + docker-compose.yml | #31 |
| 20 | [#42](https://github.com/rnataoliveira/mechanics-software/issues/42) | Vulnerability scan + report | #41 |
| 20 | [#43](https://github.com/rnataoliveira/mechanics-software/issues/43) | Record demo video (max 15 min) | #41 |
| 21 | [#44](https://github.com/rnataoliveira/mechanics-software/issues/44) | Finalize submission — collaborator, DELIVERABLES.md, PDF | #42, #43 |

---

## Parallelization Opportunities

The following groups can be worked on simultaneously by different team members:

| Group | Issues | When |
|---|---|---|
| Domain value objects | #10, #11, #12, #17, #18 | After #9 merged |
| Domain entities | #13, #14, #15, #16 | After their VO deps merged |
| Application CRUD slices | #21, #22, #23, #24, #25 | After #20 merged |
| Infra security | #29, #30 | After #20 merged |
| Unit tests (domain) | #35, #36 | Can start during M1 |
| Integration tests | #38, #39 | After #31 merged |
| Docker + scan + video | #41, #42, #43 | #42 and #43 after #41 |

---

## Milestone Summary

| Milestone | Issues | Due Date | Status |
|---|---|---|---|
| M1 — Domain Layer | #45, #9, #10, #11, #12, #17, #18, #13, #14, #15, #16, #19 | Mar 22, 2026 | Not started |
| M2 — Application Layer | #20, #21, #22, #23, #24, #25, #26 | Apr 5, 2026 | Not started |
| M3 — Infrastructure Layer | #46, #29, #30, #27, #28 | Apr 12, 2026 | Not started |
| M4 — API Layer | #34, #33, #32, #31 | Apr 19, 2026 | Not started |
| M5 — Tests | #35, #36, #37, #38, #39, #40 | Apr 26, 2026 | Not started |
| M6 — Final Deliverables | #41, #42, #43, #44 | May 1, 2026 | Not started |
