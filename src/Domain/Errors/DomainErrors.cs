using Domain.Shared;

namespace Domain.Errors;

public static class DomainErrors
{
    #region User
    
    #region Entities
    
    public static class User
    {
        public static readonly Error EmailAlreadyInUse = new(
            "User.EmailAlreadyInUse",
            "The specified email is already in use");

        public static readonly Func<Guid, Error> NotFound = id => new Error(
            "User.NotFound",
            $"The user with the identifier {id} was not found.");

        public static readonly Error NotExist = new(
            "Users.NotExist",
            $"There is no users");

        public static readonly Error InvalidCredentials = new(
            "User.InvalidCredentials",
            "The provided credentials are invalid");
    }
    
    #region RefreshToken

    public static class RefreshToken
    {
        public static readonly Error InvalidToken = new(
            "RefreshToken.InvalidToken",
            "The refresh token is invalid.");

        public static readonly Error ExpiredToken = new(
            "RefreshToken.ExpiredToken",
            "The refresh token has expired.");

        public static readonly Error RevokedToken = new(
            "RefreshToken.RevokedToken",
            "The refresh token has been revoked.");

        public static readonly Error CreationFailed = new(
            "RefreshToken.CreationFailed",
            "Failed to create refresh token.");
    }

    #endregion

    #endregion

    #region Value Objects

    public static class Email
    {
        public static readonly Error Empty = new(
            "Email.Empty",
            "Email is empty");
        public static readonly Error InvalidFormat = new(
            "Email.InvalidFormat",
            "Email format is invalid");
    }

    public static class FullName
    {
        public static readonly Error Empty = new(
            "FirstName.Empty",
            "First name is empty");
        public static readonly Error TooLong = new(
            "LastName.TooLong",
            "FirstName name is too long");
    }
    
    #endregion
    
    #endregion

    public static class General
    {
        public static readonly Error DatabaseError = 
            new("General.DatabaseError", "A database error occurred.");

        public static readonly Error Unexpected = 
            new("General.Unexpected", "An unexpected error occurred.");
    }
}
