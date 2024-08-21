//using QueryStructure;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generalizing the query creation for the custom queries 
/// </summary>
public interface IGraphQlQueryCustom
{
    /// <summary>
    /// The last query that was executed in its string form
    /// </summary>
    string LastQuery { get; set; }

    /// <summary>
    /// Where Clause dictates how the query should be filtered, given the name of the variable and their value
    /// </summary>
    List<(string name, string order)> WhereClauseArguments { get; set; }

    /// <summary>
    /// Order Clause dictates how the order of the query should be coming back as, given the name of the variable and the direction (DESC or ASC)
    /// </summary>
    (string fieldName, string direction)? OrderClauseArguments { get; set; }


    // Function to add to the where list
    public void AddToWhereClause(string name, string order)
    {
        if (WhereClauseArguments == null)
        {
            WhereClauseArguments = new List<(string name, string order)>();
        }

        WhereClauseArguments.Add((name, order));
    }
    // Function to delete the whole where clause list
    public void ClearWhereClause()
    {
        WhereClauseArguments.Clear();
    }
    // Function to fetch the whole where clause list
    public List<(string name, string order)> GetWhereClauseArguments()
    {
        return WhereClauseArguments;
    }

    // Function to add to the where list
    public void AddToOrderClause(string name, string order)
    {
        OrderClauseArguments = (name, order);
    }
    // Function to delete the whole where clause list
    public void CleaOrderClause()
    {
        OrderClauseArguments = null;  //to fix 
    }

 
    /// <returns>Returns the Current Order Clause</returns>
    public (string name, string order)? GetOrderClauseArguments()
    {
        return OrderClauseArguments;
    }

    public string CreateQueryString(string graphQlQueryFrame)
    {
        // Check if either the whereClauseArgs or the orderClauseArgs are null or empty
        string whereClause = WhereClauseArguments.Any() ? GraphQlHelper.CreateWhereClause(WhereClauseArguments) : null;
        string orderClause = OrderClauseArguments.HasValue && !string.IsNullOrEmpty(OrderClauseArguments.Value.fieldName) ? GraphQlHelper.CreateOrderClause(OrderClauseArguments.Value) : null;

        // Create the query
        var queryComponents = new List<string>();
        if (!string.IsNullOrEmpty(whereClause))
            queryComponents.Add(whereClause);
        if (!string.IsNullOrEmpty(orderClause))
            queryComponents.Add(orderClause);

        return queryComponents.Any() ? GraphQlHelper.QueryWithInput(graphQlQueryFrame, queryComponents) : GraphQlHelper.QueryNoInput(graphQlQueryFrame);
    }

}
