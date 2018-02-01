Imports PHE.SL.Interfaces
Imports PHE.Business
Imports PHE.SL.Core
Imports PHE.Common
Imports System.Text
Imports Newtonsoft.Json
Imports PHE.SL.Services

Class HawkService
    Implements IEnhancedProductCatalogService

    Private _hawkServiceBaseUrl As String   'http://{0}.hawksearch.com/sites/{1}/?output=json&'
    Private _hawkServiceEnhancedUrl As String  'http://{0}.hawksearch.com/sites/{1}/?output=custom&hawkitemlist=json&hawktoppager=json&hawkfeatured=json&	
    Private _hawkAPIKey As String
    Private _hawkEngineName As String
    Private _useEnhancedHawkSearch As Boolean
    Private _useStaging As Boolean

    Private _context As ISessionContext
    Private _siteService As ISiteService


    Public Sub New(ByVal context As ISessionContext, ByVal siteService As ISiteService)
        _context = context
        _siteService = siteService
        _useStaging = siteService.AppConfigBool("HawkSearch_UseStaging")
        _hawkAPIKey = siteService.AppConfig("HawkSearch_APIKey")
        _hawkEngineName = siteService.AppConfig("HawkSearch_EngineName")
        _hawkServiceBaseUrl = _siteService.AppConfig("HawkSearch_BaseUrl")
        _hawkServiceEnhancedUrl = siteService.AppConfig("HawkSearch_EnhancedUrl")
        _useEnhancedHawkSearch = AppConfig.AppConfigBool("UseEnhancedHawkSearch")
    End Sub



#Region "IEnhancedProductCatalogService Implementation"



    Public Sub ExecuteCategoryProductListQuery(ByVal request As GetProductListRequest, ByVal response As GetProductListResponse) Implements IEnhancedProductCatalogService.ExecuteCategoryProductListQuery

        Dim effectiveRefinements As List(Of Refinement) = Nothing
        Dim queryString = String.Empty
        'set up the query string for the API Call
        queryString = GetQueryStringAndEffectiveRefinements(request, effectiveRefinements, _useEnhancedHawkSearch)
        Dim queryResults As IHawkResults = Nothing


        'execute the main query
        queryResults = ExecuteEnhancedQuery(queryString, request.Options.PageIndex, request.Options.PageSize)

        'map query results for the necessary objects
        response.TrackingId = queryResults.TrackingId
        Dim products = GetProductsFromQueryResults(DirectCast(queryResults, IHasHawkProductIDs))

        Dim productviews = products.Select(Function(a) a.ToProductSummaryView(_context.Site)).ToList
        response.Products = New Page(Of ProductSummaryView) With {.Items = productviews,
                                                                  .TotalCount = queryResults.Pagination.NofResults,
                                                                  .PageSize = request.Options.PageSize,
                                                                  .PageIndex = request.Options.PageIndex
                                                                }

        response.Supplements = DirectCast(queryResults, IHasHawkSupplements).ToSupplementViews(_context.Site)


        If TypeOf queryResults Is HawkResult Then
            response.AvailableRefinements = GetRefinementListFromHawkResult(DirectCast(queryResults, HawkResult))
            effectiveRefinements.ForEach(Sub(refinement)
                                             Dim refToRemove = response.AvailableRefinements.Where(Function(r) r.ID = refinement.ID AndAlso r.Type = refinement.Type).FirstOrDefault
                                             If refToRemove IsNot Nothing AndAlso refToRemove.Type <> RefinementTypeEnum.Category Then
                                                 response.AvailableRefinements.Remove(refToRemove)
                                             End If
                                         End Sub)


        End If
        response.EffectiveRefinements = If(effectiveRefinements IsNot Nothing, effectiveRefinements, New List(Of Refinement))

        If Not String.IsNullOrEmpty(queryResults.Location) Then
            response.SearchTermRedirect = New SearchTermRedirect With {.RedirectURL = queryResults.Location,
                                                                        .Term = request.SearchTerm}
        End If


    End Sub


    Private Function ExecuteEnhancedQuery(queryString As String, Optional ByVal pageIndex As Integer = 1, Optional ByVal pageSize As Integer = 24) As EnhancedHawkResult
        queryString = String.Format("{0}&pg={1}&mpp={2}", queryString, pageIndex, pageSize)

        Dim cacheKey As String = "EnhancedHawkQuery" & _context.SiteCode & ":" & queryString
        Dim queryResults As EnhancedHawkResult = If(_siteService.AppConfigBool("rel-HawkCaching"), CType(DataCache.GetCache(cacheKey), EnhancedHawkResult), Nothing)

        If queryResults Is Nothing Then
            queryResults = policy.Execute(Function()
                                              Using request = New WebClient(AppConfig.AppConfigUSInt("HawkSearch-timeout"))
                                                  request.Headers("User-Agent") = _context.RequestContext.UserAgent
                                                  request.Headers("HTTP_TRUE_CLIENT_IP") = _context.RequestContext.RemoteAddress
                                                  Dim jsonString = request.DownloadString(FixURL(_hawkServiceEnhancedUrl, queryString))
                                                  Dim qr = JsonConvert.DeserializeObject(Of EnhancedHawkResult)(jsonString)
                                                  qr.Data.ItemList = JsonConvert.DeserializeObject(Of Dictionary(Of String, List(Of HawkResultItem)))(qr.Data.Results)("Items")
                                                  qr.Data.Pagination = JsonConvert.DeserializeObject(Of Dictionary(Of String, HawkPagination))(qr.Data.TopPager)("Pagination")
                                                  qr.Data.Featured = JsonConvert.DeserializeObject(Of Dictionary(Of String, HawkFeaturedItems))(qr.Data.FeaturedItems)("Items")
                                                  Return qr
                                              End Using
                                          End Function,
                                          Function() AppConfig.AppConfigBool("rel-HawkSearch-usePolicy"))

            DataCache.SetCache(cacheKey, queryResults)
        End If

        Return queryResults
    End Function




#End Region

#Region "Private methods"


    Private Function GetQueryStringAndEffectiveRefinements(ByVal request As GetProductListRequest, ByRef list As List(Of Refinement), Optional ByVal useHawkLeftNav As Boolean = False) As String
        list = New List(Of Refinement)

        Dim sb = New StringBuilder()
        Dim environmentString = If(_useStaging, "staging", "manage")

        If request.CategoryID <> 0 Then
            sb.AppendFormat("lpurl=/-c-{0}.aspx", request.CategoryID)
        ElseIf Not String.IsNullOrWhiteSpace(request.LandingPageURL) Then
            sb.AppendFormat("lpurl={0}", request.LandingPageURL)
        End If

        Dim sort As String = GetSortString(request.Options.SortOrder)

        If Not String.IsNullOrEmpty(sort) Then
            sb.AppendFormat("&{0}", sort)
        End If

        If Not AppConfig.AppConfigBool("AllowVirtualSkuProducts") OrElse _context.Site.IsRetailStore Then
            sb.Append("&is_virtual_sku=No")
        End If

        If AppConfig.AppConfigBool("SiteCodeProductExclusion", _context.SiteCode) Then
            sb.AppendFormat("&sitecode={0}", _context.SiteCode)
        End If

        If Not String.IsNullOrWhiteSpace(request.SearchTerm) Then
            sb.AppendFormat("&keyword={0}", request.SearchTerm)
        ElseIf queryParams.AllKeys.Contains("keyword") Then
            sb.AppendFormat("&keyword={0}", queryParams("keyword"))
        End If

        If _context.MobileSiteIsEnabled() Then
            sb.Append("&hawkcustom=mobile")
        ElseIf _context.TabletSiteIsEnabled() Then
            sb.Append("&hawkcustom=tablet")
        Else
            sb.Append("&hawkcustom=desktop")
        End If

        If AppConfig.AppConfigBool("rel-HawkRobotsAllow") Then
            sb.Append("&hawkrobots=allow")
        End If

        If Not request.AutoCorrect Then
            sb.Append("&hawkspellcheck=0")
        End If

        sb.Append(String.Format("&HawkSessionId={0}", _context.UserID))

        Return sb.ToString()
    End Function



    Private Function FixURL(ByVal url As String, ByVal queryString As String) As String

        url = $"{String.Format(url, If(_useStaging, "staging", "manage"), _hawkEngineName)}{If(url.EndsWith("&"), "", "&")}{queryString}"

        If AppConfig.AppConfigBool("rel-HawkSearch-SimulateDelay") Then
            Dim timeout = AppConfig.AppConfigUSInt("rel-HawkSearch-SimulateDelay-timeout")
            url = ServiceProvider.URL.FullURL($"/product/hawkProxy?delay=1&timeout={timeout}&url={_context.URLEncode(url)}")
        End If

        Return url
    End Function

#End Region

#Region "Private Classes for Settings"

    Class HawkSettings

        Private _refinementsIndex As Dictionary(Of String, Refinement)

        Public Function GetRefinementByValue(ByVal reftype As RefinementTypeEnum, ByVal name As String) As Refinement
            Dim key = reftype.ToString() & "-" & name

            If _refinementsIndex.ContainsKey(key) Then
                Return _refinementsIndex(key)
            Else
                Return Nothing
            End If
        End Function

        Private Sub BuildRefinementsIndex(ByVal groups As RefinementGroupList)
            _refinementsIndex = New Dictionary(Of String, Refinement)()

            For Each g In groups
                For Each item In g.Refinements
                    If Not _refinementsIndex.ContainsKey(g.Type.ToString() & "-" & item.Name) Then
                        _refinementsIndex.Add(g.Type.ToString() & "-" & item.Name, item)
                    End If
                Next
            Next
        End Sub

        Private _completeRefinements As RefinementGroupList
        Public Property CompleteRefinements As RefinementGroupList
            Get
                Return _completeRefinements
            End Get
            Set(value As RefinementGroupList)
                _completeRefinements = value

                BuildRefinementsIndex(value)
            End Set
        End Property

        Public Property Dimensions As Dictionary(Of RefinementTypeEnum, HawkDimension)
        Public Property TypeOverrides As List(Of Refinement)

    End Class

    Class HawkDimension
        Public Property DimensionID As Integer
        Public Property Name As String
        Public Property QueryString As String
        Public Property UseValue As Boolean
    End Class

#End Region


#Region "Helper classes for JSON deserialization of Hawk response"

    Interface IHasHawkProductIDs
        ReadOnly Property ProductIDs As List(Of Integer)
    End Interface

    Interface IHasHawkSupplements
        Property Merchandising As HawkMerchandising
        Property FeaturedItems As HawkFeaturedItems
        Property MetaRobots As String
        Property HeaderTitle As String
        Property MetaDescription As String
        Property MetaKeywords As String
        Property RelCanonical As String
        Property Title As String
        Property PageHeading As String
    End Interface
    Interface IHawkResults
        Property Location As String
        Property DidYouMean As String
        Property Results As List(Of HawkResultItem)
        Property Pagination As HawkPagination
        Property Keyword As String
        Property Original As String
        Property TrackingId As String
    End Interface

    Class HawkResult
        Implements IHawkResults
        Implements IHasHawkProductIDs
        Implements IHasHawkSupplements



        Public Property Success As Boolean
        Public Property Pagination As HawkPagination Implements IHawkResults.Pagination
        Public Property Facets As List(Of HawkFacet)
        Public Property Results As List(Of HawkResultItem) Implements IHawkResults.Results
        Public Property Selections As Dictionary(Of String, HawkSelection)
        Public Property Location As String Implements IHawkResults.Location
        Public Property DidYouMean As String Implements IHawkResults.DidYouMean
        Public Property Keyword As String Implements IHawkResults.Keyword
        Public Property Original As String Implements IHawkResults.Original
        Public Property MetaRobots As String Implements IHasHawkSupplements.MetaRobots
        Public Property HeaderTitle As String Implements IHasHawkSupplements.HeaderTitle
        Public Property MetaDescription As String Implements IHasHawkSupplements.MetaDescription
        Public Property MetaKeywords As String Implements IHasHawkSupplements.MetaKeywords
        Public Property RelCanonical As String Implements IHasHawkSupplements.RelCanonical
        Public Property Title As String Implements IHasHawkSupplements.Title
        Public Property IsInitialized As Boolean
        Public Property Merchandising As HawkMerchandising Implements IHasHawkSupplements.Merchandising
        Public Property FeaturedItems As HawkFeaturedItems Implements IHasHawkSupplements.FeaturedItems
        Public Property TrackingId As String Implements IHawkResults.TrackingId
        Public Property PageHeading As String Implements IHasHawkSupplements.PageHeading


    End Class


    Class HawkPagination

        Public NofResults As Integer
        Public CurrentPage As Integer
        Public MaxPerPage As Integer
        Public NofPages As Integer
    End Class

    Class HawkFacet

        Public FacetId As Integer
        Public Name As String
        Public FacetType As String
        Public Field As String
        Public Values As List(Of HawkFacetValue)
        Public Ranges As List(Of HawkFacetRange)

        Public ReadOnly Property IsRange As Boolean
            Get
                Return Ranges IsNot Nothing AndAlso Ranges.Count > 0
            End Get
        End Property


    End Class


    Class HawkFacetValue
        Public Label As String
        Public ID As Integer
        Public Count As Integer
        Public Value As String

    End Class

    Class HawkResultItem
        Public ID As String
        Public CustomURL As String
        Public Custom As Dictionary(Of String, String)
    End Class


    Class HawkSelection
        Public Items As List(Of HawkFacetValue)
    End Class

    Class HawkFacetRange
        Public Label As String
        Public Value As String

    End Class

    Class HawkMerchandising
        Public Items As List(Of HawkMerchandisingItem)

    End Class

    Class HawkMerchandisingItem
        Public Zone As String
        Public Html As String
    End Class

    Class HawkFeaturedItems
        Public Items As List(Of HawkFeaturedItemsZone)

    End Class

#End Region



End Class
