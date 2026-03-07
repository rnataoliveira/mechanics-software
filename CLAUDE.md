# Project Conventions — mechanics-software

## Language
- Everything is in **English**: code, docs, comments, commit messages, PR titles, variable names, file names.
- Exception: business domain terms listed in the Ubiquitous Language doc may appear in Portuguese only when they have no accurate English equivalent (e.g. "CPF", "CNPJ", "placa"). In those cases, use the Portuguese term as-is.

## Commit messages
- Follow **Conventional Commits** (https://www.conventionalcommits.org)
- Format: `<type>(<scope>): <description>`
- Types: `feat`, `fix`, `chore`, `docs`, `test`, `refactor`, `perf`, `ci`
- Scopes: `auth`, `customers`, `vehicles`, `service-orders`, `inventory`, `shared`, `infra`, `docs`
- Description: imperative, lowercase, no period at the end
- No co-author lines
- Examples:
  - `feat(service-orders): add status transition state machine`
  - `fix(inventory): prevent negative stock on part confirmation`
  - `docs: update event storming with hot spots`
  - `test(customers): add unit tests for CPF/CNPJ value object`

## File and folder naming
- Files: `kebab-case.ts`
- Classes: `PascalCase`
- Variables and functions: `camelCase`
- Constants: `UPPER_SNAKE_CASE`
- Database tables: `snake_case` (plural)
- Env vars: `UPPER_SNAKE_CASE`

## Code style
- TypeScript strict mode enabled
- No `any` types
- Prefer explicit return types on public methods
- Use `readonly` on entity properties where applicable
- Domain entities must not depend on framework or ORM imports
- Validation belongs in Value Objects and DTOs, not in use cases

## Project structure
- Modular monolith organized by bounded context under `src/modules/`
- Each module has: `application/`, `domain/`, `infrastructure/`, `presentation/`
- Shared kernel lives in `src/shared/`
- See `docs/architecture/overview.md` for full details

## Testing
- Unit tests live alongside the file they test: `foo.spec.ts`
- Integration tests live in `test/integration/`
- E2E tests live in `test/e2e/`
- Minimum 80% coverage on domain and application layers
- Test descriptions in English, in plain language

## Documentation
- All docs live in `docs/`
- Architecture decisions go in `docs/decisions/` as ADR files
- Domain docs go in `docs/domain/`
- Diagrams and flows in `docs/domain/` as Markdown

## Git workflow
- `main` is protected: requires 1 PR approval, no force pushes
- Branch naming: `<type>/<short-description>` (e.g. `feat/create-service-order`, `fix/stock-reservation`)
- Always open a PR to merge into `main`
- Squash commits when merging if the branch has noisy history
