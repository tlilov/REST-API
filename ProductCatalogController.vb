Imports System.web.Http
Imports System.web.Http.Cors
Imports PHE.SL.Interfaces

Namespace Controllers.API
    '<EnableCors("http://127.0.0.1:8080", "*", "*", SupportsCredentials:=True)>
    <PheApiAuthorize()>
    Public Class ProductCatalogController
        Inherits ApiController
        Implements IProductCatalogService

        Private _service As IProductCatalogService

        Public Sub New(ByVal service As IProductCatalogService)
            _service = service
        End Sub

        <HttpPost()>
        Public Function GetProduct(ByVal request As GetProductRequest) As GetProductResponse Implements IProductCatalogService.GetProduct
            Return _service.GetProduct(request)
        End Function

        <HttpPost()>
        Public Function GetCategory(ByVal request As GetCategoryRequest) As GetCategoryResponse Implements IProductCatalogService.GetCategory
            Return _service.GetCategory(request)
        End Function

        <HttpPost()>
        Public Function GetRootCategories(ByVal request As GetCategoryRequest) As GetRootCategoriesResponse Implements IProductCatalogService.GetRootCategories
            Return _service.GetRootCategories(request)
        End Function

        <HttpPost()>
        Public Function GetSpecsByType(ByVal request As GetSpecsRequest) As GetSpecsResponse Implements IProductCatalogService.GetSpecsByType
            Return _service.GetSpecsByType(request)
        End Function

        <HttpPost()>
        Public Function GetSpec(ByVal request As GetSpecsRequest) As GetSpecsResponse Implements IProductCatalogService.GetSpec
            Return _service.GetSpec(request)
        End Function

        <HttpPost()>
        Public Function GetActor(ByVal request As GetActorRequest) As GetActorResponse Implements IProductCatalogService.GetActor
            Return _service.GetActor(request)
        End Function

        <HttpPost()>
        Public Function GetGuidedNavigation(ByVal request As GetGuidedNavigationRequest) As GetGuidedNavigationResponse Implements IProductCatalogService.GetGuidedNavigation
            Return _service.GetGuidedNavigation(request)
        End Function

        <HttpPost()>
        Public Function GetProductList(ByVal request As GetProductListRequest) As GetProductListResponse Implements IProductCatalogService.GetProductList
            Return _service.GetProductList(request)
        End Function

        <HttpPost()>
        Public Function GetPredictiveSearch(ByVal request As GetPredictiveSearchRequest) As GetPredictiveSearchResponse Implements IProductCatalogService.GetPredictiveSearch
            Return _service.GetPredictiveSearch(request)
        End Function

        <HttpPost()>
        Public Function GetNamedProductList(ByVal request As GetNamedProductListRequest) As GetNamedProductListResponse Implements IProductCatalogService.GetNamedProductList
            Return _service.GetNamedProductList(request)
        End Function

        <HttpPost()>
        Public Function GetProductListBySpec(ByVal request As GetProductListBySpecRequest) As GetProductListResponse Implements IProductCatalogService.GetProductListBySpec
            Return _service.GetProductListBySpec(request)
        End Function

        <HttpPost()>
        Public Function GetCollection(ByVal request As GetCollectionRequest) As GetCollectionResponse Implements IProductCatalogService.GetCollection
            Return _service.GetCollection(request)
        End Function

        Public Function IsGuidedNavigationEnabled() As Boolean Implements IProductCatalogService.IsGuidedNavigationEnabled
            Return _service.IsGuidedNavigationEnabled()
        End Function

        Public Function GetEstimatedShippingDates(request As GetEstimatedShippingDatesRequest) As GetEstimatedShippingDatesResponse Implements IProductCatalogService.GetEstimatedShippingDates
            Throw New NotImplementedException()
        End Function
    End Class

End Namespace
