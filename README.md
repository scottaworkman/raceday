# raceday
Facebook web application with REST API for Jordan YMCA group.

Uses Facebook Oauth Authentication to authenticate the user.  Uses Facebook Graph API to verify the user is a member of the Jordan YMCA
closed Facebook group and to retrieve basic information regarding the user.

Added REST API to support the <a href="https://github.com/scottaworkman/raceday-mobile">RaceDay Mobile</a> client application. API methods
include:
<ul>
<li>/LOGIN (POST) - user information and API key.  Assumes Facebook OAuth has been completed by the client</li>
<li>/EVENT (GET) - retrieve list of upcoming events with user participation indicated</li>
<li>/EVENT/{ID} (GET) - retrieve specific event details and participants</li>
<li>/EVENT (POST) - add new event</li>
<li>/EVENT/{ID} (PUT) - update existing event</li>
<li>/EVENT/{ID} (DELETE) - remove existing event</li>
<li>/ATTEND (GET) - retrieve upcoming events authenticated user is attending</li>
<li>/ATTEND/{ID} (PUT) - add user as attending the specified event</li>
<li>/ATTEND/{ID} (DELETE) - remove user as attending specified event</li>
</ul>

#Authentication
Assumes client has authenticated user with Facebook OAuth and retrieved the Facebook UserId

POST /API/LOGIN<br />
{ groupid: [JYMF Facebook group Id], userid: [Facebook User Id], apikey: [Application API Key] }<br />
Returns:<br />
{ token: [access token], expiration: [date/time token expires], role: [(-1=empty | 1=denied | 5=member | 10=admin)] }<br />

All REST calls should include the following header:<br />
Authorization:  Bearer [access token]

