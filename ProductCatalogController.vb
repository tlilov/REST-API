Imports System.web.Http
Imports System.web.Http.Cors
Imports PHE.SL.Interfaces

Namespace Controllers.API
  
    <PheApiAuthorize()>
    Public Class ProductCatalogController
        Inherits ApiController
        Implements IProductCatalogService

        Private _service As IProductCatalogService

        Public Sub New(ByVal service As IProductCatalogService)
            _service = service
        End Sub

      
        <HttpPost()>
        Public Function GetNamedProductList(ByVal request As GetNamedProductListRequest) As GetNamedProductListResponse Implements IProductCatalogService.GetNamedProductList
            Return _service.GetNamedProductList(request)
        End Function

       
    End Class

End Namespace
