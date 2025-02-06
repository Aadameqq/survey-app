using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class GetCurrentAccountUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly GetCurrentAccountUseCase useCase;

    public GetCurrentAccountUseCaseTests()
    {
        useCase = new GetCurrentAccountUseCase(accountsRepositoryMock.Object);

        accountsRepositoryMock
            .Setup(x => x.FindById(existingAccount.Id))
            .ReturnsAsync(existingAccount);
    }

    [Fact]
    public async Task WhenAccountWithGivenIdDoesNotExist_ShouldFail()
    {
        var result = await useCase.Execute(Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoRepositoryChanges();
    }

    [Fact]
    public async Task WhenAccountWithGivenIdExists_ShouldSucceedAndReturnAccount()
    {
        var result = await useCase.Execute(existingAccount.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(existingAccount, result.Value);
    }

    private void AssertNoRepositoryChanges()
    {
        accountsRepositoryMock.Verify(x => x.Flush(), Times.Never);
    }
}
