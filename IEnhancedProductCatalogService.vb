Imports PHE.SL.Interfaces

Public Interface IEnhancedProductCatalogService

    ReadOnly Property IsEnabled As Boolean
    Sub ExecuteCategoryProductListQuery(request As GetProductListRequest, response As GetProductListResponse)
        
    Function ExecuteNamedProductListQuery(request As GetNamedProductListRequest) As GetNamedProductListResponse
            
    Function GetCompleteRefinements() As List(Of RefinementGroup)
                
    Sub ExecutePredictiveSearchQuery(request As GetPredictiveSearchRequest, response As GetPredictiveSearchResponse)

End Interface
