using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GraphQlHelper
{
    //the current problem is that this code does not allow for inequalities just simple if equal to filters on the where clause
    // 128 u256 contract felt they need to be strings

    //TODO: Add support for inequalities in the where clause, this should be treated as an enum theoretically

    /// <summary>
    /// Turns the query backbone into a query that does not require any input
    /// </summary>
    /// <param name="queryBackboneStructure"></param>
    /// <returns></returns>
    public static string QueryNoInput(string queryBackboneStructure)
    {
        return queryBackboneStructure.Replace("arguments", "");
    }

    /// <summary>
    /// This creates the correct query for the graphql query, given the query backbone and the list of clauses
    /// </summary>
    /// <param name="queryBackboneStructure">Which query structure from which model to use</param>
    /// <param name="clauses">The argument clause to push through, currently only order and where are supported</param>
    /// <returns></returns>
    public static string QueryWithInput(string queryBackboneStructure, List<string> clauses)
    {
        // this needs some error checking
        var argumentQuery = "(";
        for (int i = 0; i < clauses.Count; i++)
        {
            argumentQuery += clauses[i];
            if (i < clauses.Count - 1)
            {
                argumentQuery += ", ";
            }
        }
        argumentQuery += ")";
        string query = queryBackboneStructure.Replace("arguments", argumentQuery);
        return query;
    }

    /// <summary>
    /// Given a List of type 
    /// </summary>
    /// <param name="whereArguments"></param>
    /// <returns>Retruns the built string for the argument to have a filter in the Query, pass this into the QueryWithInput list argument</returns>
    public static string CreateWhereClause(List<(string name, string order)> whereArguments)
    {
        string whereClause = "where: {";
        foreach (var pair in whereArguments)
        {
            whereClause += pair.name + ": " + pair.order + ", ";
        }
        whereClause = whereClause.Remove(whereClause.Length - 2);
        whereClause += "}";
        return whereClause;
    }

    //test what happends if given more than one orded clause in the graphql playground
    /// <summary>
    /// 
    /// </summary>
    /// <param name="replacements">Touple containing 2 strings, first string is the field name, this should be the name of the variable being sorted for in ALL CAPS. 
    /// second string is the direction either DESC or ASC and the other </param>
    /// <returns>Returns the built string for the argument to have an order in the Query, pass this into the QueryWithInput list argument</returns>
    public static string CreateOrderClause((string fieldName, string direction) replacements)
    {
        return $"order: {{ direction: {replacements.direction}, field: {replacements.fieldName} }}";
    }
}
