using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that uses the interface IGraphQlQueryCustom, this component should mainly be used for places like sorting of the market place.
/// the point is to make it easy to add the where and order clause to the query and then call it
/// </summary>
public class GraphQlQueryCustomInteractor : MonoBehaviour
{
    // this can be used in the query manager as a type anyway, so this needs to be updated in the tool @Alex, also its missing the enum implementation
    [System.Serializable]
    public class WhereClauseArgument
    {
        public string Name;
        public string Value;
    }

    [SerializeField, SerializeReference]
    public IGraphQlQueryCustom graphQlQueryCustom;

    [Space(20)]
    [Header("Order Argument")]
    public string OrderFieldName;
    public string OrderDirection;

    //so this on top of it, this would do the basic stuff liek for example status and maybe game
    //but then for example the player or specific data is added later
    [Space(20)]
    [Header("Where Argument")]
    [SerializeField]
    private List<WhereClauseArgument> whereClauseArguments;

    public void AddToWhereClause(string name, string value)
    {
        if (whereClauseArguments == null)
        {
            whereClauseArguments = new List<WhereClauseArgument>();
        }

        whereClauseArguments.Add(new WhereClauseArgument { Name = name, Value = value });
    }

    public void ClearWhereClause()
    {
        foreach (var item in whereClauseArguments)
        {
            graphQlQueryCustom.AddToWhereClause(item.Name, item.Value);
        }
    }

    public void AddToOrderClause()
    {
        graphQlQueryCustom.AddToOrderClause(OrderFieldName, OrderDirection);
    }
}
