Imports System.web.Http
Imports System.web.Http.Cors
Imports PHE.SL.Interfaces

Namespace Controllers.API
  
    <PheApiAuthorize()>
    Public Class ProductCatalogController
        Inherits ApiController
        Implements IProductCatalogService

        Private _service As IEnhancedProductCatalogService
 
        Public Sub New(ByVal service As IEnhancedProductCatalogService)
            _service = service
        End Sub

      
        <HttpPost()>
        Public Function GetNamedProductList(request As Interfaces.GetNamedProductListRequest) As GetNamedProductListResponse Implements IProductCatalogService.GetNamedProductList
                    
                    Return _service.ExecuteCategoryProductListQuery(request)
    
        End Function

       
    End Class

End Namespace
