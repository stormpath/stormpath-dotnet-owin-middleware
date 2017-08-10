# Changelog

The 4.0 series of Stormpath.Owin will help you migrate an application [from Stormpath to Okta](https://stormpath.com/oktaplusstormpath). This will be the last major release of this project; future support for Okta-powered applications will live in a different project.

We've tried to make it as easy as possible to move a .NET application backed by Stormpath to Okta, but not everything works the same way. Some applications will work as-is, and some will require refactoring. Refer to the [migration guide](migrating.md) for more information.

### Who should use this library

This library and information is relevant if:

 - You are a Stormpath customer that is migrating to Okta [(learn more)](https://stormpath.com/oktaplusstormpath).
 - You have successfully exported your tenant data from Stormpath [(learn more)](https://stormpath.com/export).
 - You plan to imported your data into Okta [(learn more)](https://developer.okta.com/documentation/stormpath-import).

If you fall into one of these categories, please read the information below (and in the Migration Guide) to understand what features have changed, and what features are the same. If you have questions or need help, please reach out to us at developers@okta.com.

### Migration guide

Follow the [migration guide](migrating.md) to understand how to migrate an application step-by-step.

### Stormpath features that will not migrate

See the Compatibility Matrix on the [Stormpath-Okta Customer FAQ](https://stormpath.com/oktaplusstormpath) for a complete list of features that are not being migrated. The relevant points for this library are:

* Organizations and multitenancy is handled differently in Okta. If your application utilizes the Organization resource, please contact developers@okta.com so we can help you find a solution.
* ID Site will not work with Okta. If you are using ID Site, reach out to developers@okta.com for help.
* Custom Data is only be available on account resources.
* The Verification Success Email, Welcome Email, and Password Reset Success Email workflows are not supported.

### Note about password reset

The password reset (`/forgot` and `/change` routes) rely on a code saved to the Okta user profile in the `stormpathMigrationRecoveryAnswer` field.

If you import users from Stormpath or create new users through the `/register` route in this middleware, this is handled for you automatically.

If you create new Okta users manually (either through the developer dashboard or API), you'll need to add a random code to this field yourself. For an example, see [this gist](https://gist.github.com/nbarbettini/a97a71c7d21ee5a3cd998a3e90c45370).

## Version 4.1.0

No breaking changes.

Some nonbreaking fixes and improvements:

* Restored limited caching support. Caching is disabled by default. Caching works with User resources only and is intended to speed up the response time of the middleware. Any cache system that supports the Microsoft Caching Extensions interface is supported. To turn on and configure caching, see [this code snippet](https://github.com/stormpath/stormpath-dotnet-owin-middleware/blob/master/test/Stormpath.Owin.NowinHarness/Program.cs#L66-L70).

* Improved error messages in the registration and password reset flows when the user tries to save a password that isn't complex enough. It's possible to customize these error messages through the `IFriendlyErrorTranslator` interface; for more details, see [PR #97](https://github.com/stormpath/stormpath-dotnet-owin-middleware/pull/97).

* Added the `web.refreshTokenCookie.maxAge` property to the configuration object, which controls the expiration time (Max-Age) of the refresh token cookie. If this isn't set, the refresh token cookie will expire when the browser is closed (Session expiration). This is a breaking change from Stormpath behavior, but not a breaking change from v4.0.0 of this library.

* Added the `authorizationServerId` property to the configuration object, which you can set to explicitly select the Okta Authorization Server to use. If it is null or empty, the middleware will attempt to discover the imported Authorization Server to use (as before).

* Added the `web.oauth2.password.defaultScope` property to the configuration object, which gives your application more control over which scopes are automatically requested when a client uses the `/oauth/token` route exposed by the middleware. In v4.0.0, the scope was hardcoded to `openid offline_access`. If you don't want a refresh token issued automatically, change this setting to `openid`.

## Version 4.0.0

No breaking changes from RC5.

Some internal cleanup:

* Removed the `StormpathTokenExchanger` and `SocialExecutor` classes (unused)
* Added an `aud` check to the local token verifier

## Version 4.0.0-RC5

* The `/oauth/token` route will now return `400 Bad Request` if the username or password fields are missing. This shouldn't be a breaking change compared to the previous Stormpath functionality, but earlier versions of the migration code failed with a less-helpful error message here.

* The `/me` endpoint will now return the user's groups if `expand: groups` is set in the configuration (this unbreaks a previously-breaking change). Compared to previous Stormpath functionality, the embedded group object does not have an `href` (it has an `id` instead), and the Status property is always `enabled` (because groups in Okta cannot be disabled).

## Version 4.0.0-RC4

No breaking changes. A few bug fixes, detailed in the [release notes](https://github.com/stormpath/stormpath-dotnet-owin-middleware/releases/tag/4.0.0-rc4).

## Version 4.0.0-RC3

### Breaking changes

* The Client Credentials grant works, but it is handled differently in Okta than it was at Stormpath. The API key ID and secret are stored in the user profile, and are verified locally by this middleware code. This feature is intended to help our customers migrate, but won't be how Okta supports API key management going forward. If you're using the API key management features of Stormpath heavily, please reach out to support@stormpath.com and let us know so we can assist.

## Version 4.0.0-RC2

### Breaking changes

* Authorizing (using attributes in ASP.NET or handlers in ASP.NET Core) by Group `href` is no longer possible. Authorizing by Group name still works.
* Any `OrganizationNameKey` value that is set in a pre-handler is not honored.

#### Social login

* Social login has been revamped to use Okta and OpenID Connect.
* Social login providers are no longer returned in the `/login` JSON view model (only in the HTML response).
* The `AccountStoreProviderViewModel` object has been removed.
* It is no longer possible to set separate callback URIs per social provider. All social login requests will go through Okta and back to the `/stormpathCallback` route.
* It is no longer possible to determine if a social account is registering (new) or logging in (returning). All social accounts are treated as returning accounts.


## Version 4.0.0-RC1

### Breaking changes

* The minimum supported .NET Framework version is now 4.5.1 (previosuly 4.5).
* Instead of providing the Stormpath API key ID and secret via configuration, you'll need to provide an Okta org URL, API token, and application ID. See the [migration guide](migrating.md) for more information.
* The Stormpath SDK has been removed. If you weren't using the SDK directly, this shouldn't impact you. If you were, you'll need to refactor the relevant code to use the Okta .NET SDK or REST API calls.
* The SDK `IAccount` interface is no longer used to represent a Stormpath account profile. The `ICompatibleOktaAccount` interface is used instead. This interface has the same top-level profile properties as the Stormpath `IAccount` object (mapped to the appropriate Okta profile properties), and includes an `OktaUser` property that can be used to directly access the Okta user properties.
* Custom Data is no longer a linked resource. It's now treated as a simple dictionary on the `ICompatibleOktaUser` object (or the Okta user object). 
* Okta handles custom profile fields differently than Stormpath. Any custom profile field you want to use must be defined in advance in the Universal Directory Profile. Otherwise, you will get API errors when creating a user with a custom profile field.
* The only expansion options that currently work for the `/me` route are `customData` and `groups`. The `groups` option expands the user's Okta groups, which are similar to Stormpath groups (but have an `id` instead of an `href` field).

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
