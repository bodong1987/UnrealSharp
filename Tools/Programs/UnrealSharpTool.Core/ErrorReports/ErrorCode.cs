namespace UnrealSharpTool.Core.ErrorReports;

public enum WarningCode
{
    NoWarning
}


public enum ErrorCode
{
    NoError,

    UnsupportedProperty,
    UnsupportedMethodReturnType,
    UnsupportedMethodParameter,
    MissingUFunctionAttribute,
    InvalidBaseClass,
    InvalidTypeName,
    UClassCannotBeAbstract,
    UClassCannotBeSealed,
    UClassCannotBeStatic,
    RawUClassPointerIsNotSupported,
    NullableValueTypeIsNotSupported,
    InvalidEnumSize,
    UFunctionOverloadIsNotSupported,
    UFunctionInFunctionLibraryMustBeStatic,
    UEnumSpecifyFieldValueIsNotSupported,
    SingleCastDelegateIsNotSupported,
    SoftObjectPtrInStructIsNotSupported,
    SoftClassPtrInStructIsNotSupported,
    DelegateInStructIsNotSupported,
    InvalidPropertyAttribute,
    FunctionLibraryCannotAllowAnyProperties,
    StaticUPropertyIsNotSupported,
    StaticUFunctionCanOnlyBeUsedInFunctionLibrary,
    GenericUnrealTypeIsNotSupported,
    GenericUFunctionIsNotSupported
}