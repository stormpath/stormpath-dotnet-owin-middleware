# Changelog

## Version 4.0.0-RC1

The 4.0 release of Stormpath.Owin supports applications migrating [from Stormpath to Okta](https://stormpath.com/blog/stormpaths-new-path). This will be the last major release of this project; future support for Okta-powered applications will live in a different project.

We've tried to make it as easy as possible to move a .NET application backed by Stormpath to Okta, but not everything works the same way. Some applications will work as-is, and some will require refactoring. **The breaking and potentially-breaking changes are listed below.**

If you have questions or need help, please reach out to us at support@stormpath.com.

### Coming soon

These features don't work yet, but are coming in a future RC.

* Authorization filters (attributes)
* Social login
* Client Credentials (API key/secret) authentication
* Updating user profile or custom fields (reading works, no way to save currently)

### Breaking changes

* You must provide these new configuration properties:
	* `org` (your Okta org URL, like `https://dev-123456.oktapreview.com`),
	* `apiToken` (an Okta [API token](http://developer.okta.com/docs/api/getting_started/getting_a_token.html)),
	* `application.id` (the Okta application ID, which can be found in the URL of the Admin UI when editing the Application: `/admin/app/oidc_client/instance/<appid>`)
* If you were using `STORMPATH_*` environment variables to set any configuration properties, you'll need to update them to `OKTA_*`.
* The Stormpath SDK has been removed. If you weren't accessing the SDK directly, this shouldn't impact you. If you were, you will need to refactor the relevant code to use the Okta .NET SDK or REST API calls.
* The SDK `IAccount` interface is no longer used to represent a Stormpath account profile. The `ICompatibleOktaAccount` interface is used instead. This interface has the same top-level profile properties as the Stormpath `IAccount` object (mapped to the appropriate Okta profile properties), and includes an `OktaUser` property that can be used to directly access the Okta user properties.
* Custom Data is no longer a linked resource. It's now treated as a simple dictionary on the `ICompatibleOktaUser` object (or the Okta user object). 
* Okta handles custom profile fields differently than Stormpath. Any custom profile field you want to use must be defined in advance in the Universal Directory Profile. Otherwise, you will get API errors when creating a user with a custom profile field.
* The only expansion option that currently works for the `/me` route is `customData`.

* The `/forgot` and `/change` routes are now **disabled** by default. The routes can be enabled or disabled by changing the `web.forgotPassword.enabled` or `web.changePassword.enabled` settings.

#### Password reset

* You will need to re-create the email template for the password reset email.  You can copy the current template from the Stormpath Admin Console, then in the Okta console you can paste it into the template found at Settings > Email & SMS > Forgot Password.  You'll want to use the ``${recoveryToken}`` variable to create a link that points the user to the change password endpoint on your application, for example: ``http://localhost:3000/change?sptoken=${recoveryToken}``.
* The custom profile field `stormpathMigrationRecoveryAnswer` (string) must be defined in your Okta Universal Directory. This package uses it internally for the forgot password flow. (If you used the Stormpath import tool, this should be done for you automatically.)

#### Email verification

* Okta cannot yet send an email for the email verification flow automatically. Your application will need to send this email by providing an implementation for `SendEmailVerificationHandler`. (TODO example)
* The email verification requirement for new accounts must now be explicitly enabled using the new `web.register.emailVerificationRequired` setting.
* If `web.register.emailVerificationRequired == true`, the custom profile field `emailVerificationToken` (string) must be defined in your Okta Universal Directory.
* The custom profile fields `emailVerificationStatus` (string) must be defined in your Okta Universal Directory.

#### Potentially-breaking changes

* Okta uses an API Token to authenticate calls to the Okta API, similar to Stormpath's API Key ID/Secret.  However, unlike Stormpath API credentials, Okta API Tokens will expire in 30 days if they are not used. This means you will get an API error if your application has not been started in 30 days. If this happens, you can generate a new API Token in the Okta Admin dashboard.
* The `StateTokenBuilder` and `StateTokenParser` classes were moved from `Stormpath.Owin.Abstractions` to `Stormpath.Owin.Middleware`.
* The `*FormViewModelBuilder` classes were moved from `Stormpath.Owin.Abstractions` to `Stormpath.Owin.Middleware`.
* Configuration provided via an `appsettings.json` file must now be placed in an `Okta` section. (See other [configuration-related changes](TODO)).