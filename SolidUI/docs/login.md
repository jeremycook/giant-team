# Login

The goal of login is to authenticate the user in a way that minimizes account takeover risks while not being unduly burdensome.

# Authentication flows

First-time authentication

1. Ask for a login identifier (currently email address but phone number may be supported in the future)
2. If the login identifier is associated with a user:
    a. Send them a verification code
    b. Provide the client a verification nonce
    c. Ask to login with their identifier, password, verification code, and whether to remain logged in
3. If the login identifier is not associated with a user:
    a. Send them a verification code
    b. Provide the client a verification nonce
    b. Ask to register with their identifier, password, verification code, and whether to remain logged in
4. If they did not opt-into remaining logged in then authenticate the user for the lesser of 12 hours or 30 minutes of inactivity, and the device-user-pair for 12 hours
5. If they opted to trust this device then authenticate the user for 30 days, and the device-user-pair for 90 days
6. Redirect to page

Reauthentication

1. If the device-user-pair is not found, is invalid or has expired, follow the first-time authentication path
2. Otherwise, ask to login with their identifier and password
4. If they did not opt-into remaining logged in then authenticate the user for the lesser of 12 hours or 30 minutes of inactivity, and the device-user-pair for 12 hours
5. If they opted to trust this device then authenticate the user for 30 days, and the device-user-pair for 90 days
6. Redirect to page

Logout

1. TODO

Threats

* HTTP, if transmitted over unencrypted channels can be intercepted or modified. Always use HTTPS.
* Cookies, if transmitted over unencrypted channels can be intercepted (e.g. Firesheep). Flag authentication cookies as Secure.
* JavaScript can read cookies. Flag authentication cookies as HttpOnly
* JavaScript cannot read HttpOnly cookies but the server will read cookies set by JavaScript. Only trust authentication cookies that were signed by the server.
* Credential spraying. Require a verification code and password when authenticating from an unrecognized device.
* Online brute force password and/or verification code. Require a verification code and password when authenticating from an unrecognized device. Rate limit all authentication attempts.
* Phishing password. Require a verification code and password when authenticating from an unrecognized device. The correct password repeatedly used with an incorrect verification code indicates a compromised password and an attempt at brute forcing the verification code.
* Phishing verification code. Bind the verification code to the current authentication session so that an attacker cannot use the verification code that was generated for another device.
* Compromised verification method.
* Offline brute force password. Require at least 8 character password that has been checked against a current database of common and breached passwords, and hashed by a method that is at least as strong as 10K rounds of PBKDF2.
