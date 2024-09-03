# IdleAPI
 API for IDLE game 
<hr> 
<be> This is a code-first approached API which means it includes EF Core migration files from Models.
 
<h2>Endpoints: </h2>

<h3>[GET] /api/v1/auth?user_id= : </h3> Authenticate the user and returns an access token (<b>No JWT implementation</b>)
<h3>[GET] /api/v1/me?access_token= : </h3> Returns the user's balance and his current game session's info
<h3>[Post] /api/v1/start?access_token= : </h3> Starts a session for the user and returns the balance and the session with new parameter
<h3>[Post] /api/v1/claim?access_token= : </h3> Claim the reward when the session ends after the configured session period
<h3>[Put] /api/v1/config : </h3> Takes access token and checks whether the user is an admin or not, then takes the configuration from the request's body and updates the game's config values with it. Currently, 2 values exist: session_time (Period of each game session) & reward_value (Value of reward).


