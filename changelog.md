# Changelog

Migration information TODO.

## Breaking changes

* The Stormpath SDK has been removed under the hood. If you weren't accessing the SDK directly, this shouldn't impact you. If you were, you will need to refactor the relevant code to use the Okta .NET SDK or REST API.
* Because of this, the SDK `IAccount` interface is no longer used to represent a Stormpath account profile. To help cope with this change,
	* The user profile will be represented by `dynamic` instead of `IAccount`
	* The `dynamic` account will contain the new Okta profile fields under `account.profile`, and **also**
	* The old Stormpath top-level profile fields will be kept wherever possible
	* CustomData _todo_
* The `StateTokenBuilder` and `StateTokenParser` classes were moved from `Stormpath.Owin.Abstractions` to `Stormpath.Owin.Middleware`.
* The `*FormViewModelBuilder` classes was moved from `Stormpath.Owin.Abstractions` to `Stormpath.Owin.Middleware`.