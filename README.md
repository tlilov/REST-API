# REST-API Hawk Search - VB.Net Implementation

The Hawk Search platform enables online retailers and publishers the ability to drive a rich, compelling user search and navigation experience. This experience drives visitors to the products and information that they seeking.
https://api.hawksearch.info/api/


### Prerequisites

Hawk Search offers a REST API to manage integration with your site. It uses the four HTTP methods GET, POST, PUT and DELETE to execute different operations on landing pages.

### Run

Step 1. Create Interface IEnhancedProductCatalogService.vb around the methods from the API that we need. 

Step 2. Implement the intereface from step 1, in HawkService.vb

Step 3. In the implementation class create the proper query, based on the API Access Urls: https://api.hawksearch.info/api/

Step 4. Load and map the results from the query into the proper corrensponding client objects.

Step 5. Provide logging and error handling around each API method call.

Step 6. Write Unit tests for the API methods.

Step 7. Use a client (web app, ajax, windows app etc) to call the ExecuteCategoryProductListQuery methods that expose the REST API.
