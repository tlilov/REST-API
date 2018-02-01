Imports PHE.SL.Interfaces

Public Interface IEnhancedProductCatalogService

    ReadOnly Property IsEnabled As Boolean
    Sub ExecuteCategoryProductListQuery(request As GetProductListRequest, response As GetProductListResponse)
    Sub ExecuteNamedProductListQuery(request As GetNamedProductListRequest, response As GetNamedProductListResponse)
    Function GetCompleteRefinements() As List(Of RefinementGroup)
    Sub ExecutePredictiveSearchQuery(request As GetPredictiveSearchRequest, response As GetPredictiveSearchResponse)

End Interface
