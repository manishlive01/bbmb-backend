-- BudgetP1RupeesAndMoneyColumns, design §4.5 step 1. Domains verbatim from DDD §1.7.
--   budget.rupees: numeric(15,2) with CHECK (VALUE = trunc(VALUE) AND abs(VALUE) <= 9999999999999).
--   budget.rupees_paise: numeric(17,2), figures from Financial Accounting held unrounded (BUD-266).
CREATE DOMAIN budget.rupees AS numeric(15,2)
    CHECK (VALUE = trunc(VALUE) AND abs(VALUE) <= 9999999999999);

CREATE DOMAIN budget.rupees_paise AS numeric(17,2);
