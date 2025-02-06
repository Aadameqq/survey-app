using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.Interactors;

public class ActivateAccountUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();
    private readonly Mock<ActivationCodesRepository> activationCodesRepositoryMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly string existingCode = "123";

    private readonly ActivateAccountUseCase useCase;

    public ActivateAccountUseCaseTests()
    {
        useCase = new ActivateAccountUseCase(
            activationCodesRepositoryMock.Object,
            accountsRepositoryMock.Object
        );

        activationCodesRepositoryMock
            .Setup(x => x.GetUserIdAndRevokeCode(existingCode))
            .ReturnsAsync(existingAccount.Id);

        accountsRepositoryMock
            .Setup(x => x.FindById(existingAccount.Id))
            .ReturnsAsync(existingAccount);
    }

    [Fact]
    public async Task WhenCodeDoesNotExist_ShouldFail()
    {
        var result = await useCase.Execute("invalid-code");

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch>(result.Exception);
        AssertNoRepositoryChanges();
    }

    [Fact]
    public async Task WhenUserAssignedToCodeDoesNotExist_ShouldFail()
    {
        activationCodesRepositoryMock
            .Setup(x => x.GetUserIdAndRevokeCode(existingCode))
            .ReturnsAsync(Guid.Empty);

        var result = await useCase.Execute(existingCode);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoRepositoryChanges();
    }

    [Fact]
    public async Task WhenCodeIsValid_ShouldActivateAccountAndPersistIt()
    {
        Account? actualAccount = null;

        accountsRepositoryMock
            .Setup(x => x.Update(It.IsAny<Account>()))
            .Callback((Account account) => actualAccount = account);

        var result = await useCase.Execute(existingCode);

        Assert.True(result.IsSuccess);
        Assert.NotNull(actualAccount);
        Assert.True(actualAccount.HasBeenActivated());
        Assert.Equal(existingAccount.Id, actualAccount.Id);

        accountsRepositoryMock.Verify(x => x.Flush(), Times.AtLeastOnce);
    }

    private void AssertNoRepositoryChanges()
    {
        accountsRepositoryMock.Verify(x => x.Flush(), Times.Never);
    }
}
