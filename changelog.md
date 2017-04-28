# Changelog

## Version 4.0.0-RC1

The 4.0 release of Stormpath.Owin will help you migrate an application [from Stormpath to Okta](https://stormpath.com/oktaplusstormpath). This will be the last major release of this project; future support for Okta-powered applications will live in a different project.

We've tried to make it as easy as possible to move a .NET application backed by Stormpath to Okta, but not everything works the same way. Some applications will work as-is, and some will require refactoring. Refer to the [migration guide](migrating.md) for more information.

If you have questions or need help, please reach out to us at support@stormpath.com.

### Migration guide

Follow the [migration guide](migrating.md) to understand how to migrate an application step-by-step.

### Coming soon

These features don't work yet, but are coming in a future RC.

* Authorization filters (attributes)
* Social login
* Client Credentials (API key/secret) authentication
* Updating user profile or custom fields (reading works, no way to save currently)

### Stormpath features that will not migrate

See the Compatibility Matrix on the [Stormpath-Okta Customer FAQ](https://stormpath.com/oktaplusstormpath) for a complete list of features that are not being migrated. The relevant points for this library are:

* Organizations and multitenancy is handled differently in Okta. If your application utilizes the Organization resource, please contact support@stormpath.com so we can help you find a solution.
* ID Site will not work with Okta. If you are using ID Site, reach out to support@stormpath.com for help.
* Custom Data is only be available on account resources.
* The Verification Success Email, Welcome Email, and Password Reset Success Email workflows are not supported.

### Breaking changes

* The minimum supported .NET Framework version is now 4.5.1 (previosuly 4.5).
* Instead of providing the Stormpath API key ID and secret via configuration, you'll need to provide an Okta org URL, API token, and application ID. See the [migration guide](migrating.md) for more information.
* The Stormpath SDK has been removed. If you weren't using the SDK directly, this shouldn't impact you. If you were, you'll need to refactor the relevant code to use the Okta .NET SDK or REST API calls.
* The SDK `IAccount` interface is no longer used to represent a Stormpath account profile. The `ICompatibleOktaAccount` interface is used instead. This interface has the same top-level profile properties as the Stormpath `IAccount` object (mapped to the appropriate Okta profile properties), and includes an `OktaUser` property that can be used to directly access the Okta user properties.
* Custom Data is no longer a linked resource. It's now treated as a simple dictionary on the `ICompatibleOktaUser` object (or the Okta user object). 
* Okta handles custom profile fields differently than Stormpath. Any custom profile field you want to use must be defined in advance in the Universal Directory Profile. Otherwise, you will get API errors when creating a user with a custom profile field.
* The only expansion option that currently works for the `/me` route is `customData`.
* Authorizing by Group `href` is no longer possible. (Authorizing by Group name still works.)

#### Social login

* Social login has been revamped to use Okta and OpenID Connect.
* Social login providers are no longer returned in the `/login` JSON view model (only in the HTML response).
* The `AccountStoreProviderViewModel` object has been removed.
* It is no longer possible to set separate callback URIs per social provider. All social login requests will go through Okta and back to the `/stormpathCallback` route.

#### Password reset

* The `/forgot` and `/change` routes are now **disabled** by default. The routes can be enabled or disabled by changing the `web.forgotPassword.enabled` or `web.changePassword.enabled` settings.
* You will need to re-create the email template for the password reset email. See the [migration guide](migrating.md) for detailed steps.
* The custom profile field `stormpathMigrationRecoveryAnswer` (string) must be defined in your Okta Universal Directory. This package uses it internally for the forgot password flow. (If you used the Stormpath import tool, this should be done for you automatically.)
* Okta password reset tokens expire after 59 minutes by default. This can be changed in the Admin UI (Security - Policies - Account Recovery).

#### Email verification

* Okta cannot yet send an email for the email verification flow automatically. Your application will need to send this email by providing an implementation for `SendVerificationEmailHandler`. (example coming soon)
* The email verification requirement for new accounts must now be explicitly enabled using the new `web.register.emailVerificationRequired` setting.
* The custom profile field `emailVerificationStatus` (string) must be defined in your Okta Universal Directory.
* If `web.register.emailVerificationRequired == true`, the custom profile field `emailVerificationToken` (string) must be defined in your Okta Universal Directory.

#### Potentially-breaking changes

* Okta uses an API Token to authenticate calls to the Okta API, similar to Stormpath's API Key ID/Secret.  However, unlike Stormpath API credentials, Okta API Tokens will expire in 30 days if they are not used. This means you will get an API error if your application has not been started in 30 days. If this happens, you can generate a new API Token in the Okta Admin dashboard.
* The `StateTokenBuilder` and `StateTokenParser` classes were moved from `Stormpath.Owin.Abstractions` to `Stormpath.Owin.Middleware`.
* The `*FormViewModelBuilder` classes were moved from `Stormpath.Owin.Abstractions` to `Stormpath.Owin.Middleware`.
* Configuration provided via an `appsettings.json` file must now be placed in an `Okta` section. (See other [configuration-related changes](https://github.com/stormpath/stormpath-dotnet-config/blob/master/changelog.md)).
