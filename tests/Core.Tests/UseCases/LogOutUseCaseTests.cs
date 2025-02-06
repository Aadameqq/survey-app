using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class LogOutUseCaseTests
{
    private readonly Mock<AuthSessionsRepository> authSessionsRepositoryMock = new();

    private readonly AuthSession existingSession = new(
        Guid.Empty,
        DateTime.MinValue,
        "refreshToken"
    );

    private readonly LogOutUseCase useCase;

    public LogOutUseCaseTests()
    {
        useCase = new LogOutUseCase(authSessionsRepositoryMock.Object);

        authSessionsRepositoryMock
            .Setup(x => x.FindById(existingSession.Id))
            .ReturnsAsync(existingSession);
    }

    [Fact]
    public async Task WhenSessionWithGivenIdDoesNotExist_ShouldFail()
    {
        var result = await useCase.Execute(Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<AuthSession>>(result.Exception);
        AssertNoRepositoryChanges();
    }

    [Fact]
    public async Task WhenSessionWithGivenIdExists_ShouldSucceedAndRemoveSession()
    {
        AuthSession? removedSession = null;

        authSessionsRepositoryMock
            .Setup(x => x.Remove(It.IsAny<AuthSession>()))
            .Callback<AuthSession>(s => removedSession = s);

        var result = await useCase.Execute(existingSession.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(removedSession);
        Assert.Equivalent(existingSession, removedSession);

        authSessionsRepositoryMock.Verify(x => x.Flush(), Times.AtLeastOnce);
    }

    private void AssertNoRepositoryChanges()
    {
        authSessionsRepositoryMock.Verify(x => x.Flush(), Times.Never);
    }
}
