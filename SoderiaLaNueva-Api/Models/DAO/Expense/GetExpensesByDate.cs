namespace SoderiaLaNueva_Api.Models.DAO.Expense
{
    public class GetExpensesByDateRequest
    {
        public DateTime Date { get; set; }
    }

    public class GetExpensesByDateResponse
    {
        public List<ExpenseItem> Expenses { get; set; } = [];
        public class ExpenseItem
        {
            public string DealerName { get; set; } = null!;
            public string Description { get; set; } = null!;
            public decimal Amount { get; set; }
        }
    }
}