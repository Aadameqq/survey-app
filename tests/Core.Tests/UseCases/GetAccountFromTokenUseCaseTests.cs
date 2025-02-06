using Core.Dtos;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class GetAccountFromTokenUseCaseTests
{
    private readonly Mock<AccessTokenService> accessTokenServiceMock = new();

    private readonly AccessTokenPayload testPayload = new(Guid.Empty, Guid.Empty);

    private readonly GetAccountFromTokenUseCase useCase;

    private readonly string validToken = "token";

    public GetAccountFromTokenUseCaseTests()
    {
        useCase = new GetAccountFromTokenUseCase(accessTokenServiceMock.Object);

        accessTokenServiceMock
            .Setup(x => x.FetchPayloadIfValid(validToken))
            .ReturnsAsync(testPayload);
    }

    [Fact]
    public async Task WhenTokenInvalid_ShouldFail()
    {
        var result = await useCase.Execute("invalid-token");

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidToken>(result.Exception);
    }

    [Fact]
    public async Task WhenTokenValid_ShouldSucceedAndReturnTokenPayload()
    {
        var result = await useCase.Execute(validToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(testPayload, result.Value);
    }
}
