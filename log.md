# Log

## Assumptions

1. The `Payment` class does not include a `Method` property; only `BankAccount` and `Amount` are defined.
2. All dependencies (`IKycService`, `IBus`, `IServiceAdministrator`) are mocked for unit testing.
3. The `Application` object, including its `Applicant` and `Payment` properties, must be fully initialized before processing.
4. Strategy pattern is used to ensure modular handling of `ProductOne` and `ProductTwo`.

---

## Decisions

### Why the Strategy Pattern?

- **Modularity**: Encapsulates `ProductOne` and `ProductTwo` processing logic into separate, reusable, and testable strategies.
- **Flexibility**: Adding a new product requires implementing a new strategy and updating the strategy dictionary in `ApplicationProcessor`.
- **Adherence to SOLID Principles**:
  - **Single Responsibility**: Each strategy focuses on a single product's processing logic.
  - **Open/Closed Principle**: The `ApplicationProcessor` is open to extensions (e.g., new products) but closed to modification.
- **Alternatives Considered**:
  - **Factory Pattern**: Factories would only instantiate objects, not encapsulate behavior.
  - **Template Method**: This pattern enforces a fixed sequence but lacks the flexibility needed for varied product logic.
  - **Command Pattern**: Suitable for encapsulating requests but not optimal for modularizing product-specific processing logic.

---

## Observations

### CommonTests
1. **Class**: Validates the general behavior of the `ApplicationProcessor` class.
2. **Key Tests**:
   - `ApplicationProcessor_ThrowsException_For_UnsupportedProductCode`: Ensures an `InvalidOperationException` is thrown for invalid `ProductCode`.
   - `ApplicationProcessor_Processes_ValidProduct`: Confirms that valid applications are processed by the appropriate strategy.

### ProductOneTests
1. **Class**: Tests the behavior of `ProductOneProcessingStrategy`.
2. **Key Tests**:
   - `Application_for_ProductOne_creates_Investor_in_AdministratorOne`: Verifies successful investor creation using `AdministratorOne`.
   - `Application_for_ProductOne_Creates_Investor_Successfully`: Confirms the investor is created, and related events are published.
   - `Application_for_ProductOne_Fails_KYC`: Ensures that when KYC fails, the application processing is stopped, and no investor is created.

### ProductTwoTests
1. **Class**: Tests the behavior of `ProductTwoProcessingStrategy`.
2. **Key Tests**:
   - `Application_for_ProductTwo_Creates_Investor_And_Account_Successfully`: Validates the creation of investors, accounts, and payment processing using `AdministratorTwo`.
   - `Application_for_ProductTwo_Creates_Investor_Successfully`: Verifies successful investor creation for `ProductTwo`.
   - `Application_for_ProductTwo_Fails_KYC`: Ensures that when KYC fails, the process halts, and no further steps (e.g., account creation) are performed.

---

## Key Challenges

1. **NullReferenceException**:
   - Missing mock setups for `CreateAccountAsync` and `ProcessPaymentAsync` caused failures in `ProductTwoTests`.
2. **`Payment` Initialization**:
   - Tests failed due to improper initialization of the `Payment` object. This was resolved by explicitly constructing the `Payment` instance.
3. **Modular Testing**:
   - Ensuring consistent mocking for both `ProductOne` and `ProductTwo` tests while maintaining distinct strategies for each.

---

## Todo

1. Add behavioral tests for edge cases:
   - Invalid `ProductCode` handling in `ApplicationProcessor`.
   - Account creation failure for both products.
   - Payment failure for `ProductTwo`.
2. Refactor shared test setup logic (e.g., mocks, application initialization) into helper classes or methods.
3. Implement integration tests for end-to-end processing in `ApplicationProcessor`.
4. Review and optimize DI configurations for strategy registration.
5. Document product-specific requirements for `ProductOne` and `ProductTwo`.

---

## Summary of Tests

### CommonTests
- Ensures `ApplicationProcessor` delegates processing to the appropriate strategy or fails for unsupported `ProductCode`.

### ProductOneTests
- Focused on verifying the flow for `ProductOne`, including investor creation and handling of KYC failures.

### ProductTwoTests
- Tests `ProductTwo` processing, covering investor creation, account setup, and payment processing.

---

This log documents the design decisions, challenges, and improvements for implementing and testing the `ApplicationProcessor` with a strategy pattern.
