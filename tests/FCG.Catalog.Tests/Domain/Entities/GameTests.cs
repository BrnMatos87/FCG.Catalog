using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Exceptions;

namespace FCG.Catalog.Tests.Domain.Entities;

public class GameTests
{
    [Fact(DisplayName = "Validando se o título está vazio")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Validate_Title_Empty()
    {
        var result = Assert.Throws<DomainException>(() =>
            Game.Create(
                string.Empty,
                "Jogo de aventura",
                199,
                "Aventura"));

        Assert.Equal("O título do jogo é obrigatório.", result.Message);
    }

    [Fact(DisplayName = "Validando se o título excede o limite")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Validate_Title_MaxLength()
    {
        var title = new string('A', 151);

        var result = Assert.Throws<DomainException>(() =>
            Game.Create(
                title,
                "Jogo de aventura",
                199,
                "Aventura"));

        Assert.Equal("O título do jogo deve ter no máximo 150 caracteres.", result.Message);
    }

    [Fact(DisplayName = "Validando se a descrição está vazia")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Validate_Description_Empty()
    {
        var result = Assert.Throws<DomainException>(() =>
            Game.Create(
                "Sonic",
                string.Empty,
                199,
                "Aventura"));

        Assert.Equal("A descrição do jogo é obrigatória.", result.Message);
    }

    [Fact(DisplayName = "Validando se a descrição excede o limite")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Validate_Description_MaxLength()
    {
        var description = new string('A', 501);

        var result = Assert.Throws<DomainException>(() =>
            Game.Create(
                "Sonic",
                description,
                199,
                "Aventura"));

        Assert.Equal("A descrição do jogo deve ter no máximo 500 caracteres.", result.Message);
    }

    [Fact(DisplayName = "Validando se o preço é zero")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Validate_Price_Zero()
    {
        var result = Assert.Throws<DomainException>(() =>
            Game.Create(
                "Sonic",
                "Jogo de aventura",
                0,
                "Aventura"));

        Assert.Equal("O preço do jogo deve ser maior que zero.", result.Message);
    }

    [Fact(DisplayName = "Validando se o preço é negativo")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Validate_Price_Negative()
    {
        var result = Assert.Throws<DomainException>(() =>
            Game.Create(
                "Sonic",
                "Jogo de aventura",
                -10,
                "Aventura"));

        Assert.Equal("O preço do jogo deve ser maior que zero.", result.Message);
    }

    [Fact(DisplayName = "Validando se a categoria está vazia")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Validate_Category_Empty()
    {
        var result = Assert.Throws<DomainException>(() =>
            Game.Create(
                "Sonic",
                "Jogo de aventura",
                199,
                string.Empty));

        Assert.Equal("A categoria do jogo é obrigatória.", result.Message);
    }

    [Fact(DisplayName = "Validando se a categoria excede o limite")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Validate_Category_MaxLength()
    {
        var category = new string('A', 101);

        var result = Assert.Throws<DomainException>(() =>
            Game.Create(
                "Sonic",
                "Jogo de aventura",
                199,
                category));

        Assert.Equal("A categoria do jogo deve ter no máximo 100 caracteres.", result.Message);
    }

    [Fact(DisplayName = "Validando se o jogo foi criado com sucesso")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Create_Success()
    {
        var game = Game.Create(
            " Sonic ",
            " Jogo de aventura ",
            199,
            " Aventura ");

        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.Equal("Sonic", game.Title);
        Assert.Equal("Jogo de aventura", game.Description);
        Assert.Equal(199, game.Price);
        Assert.Equal("Aventura", game.Category);
        Assert.Equal(StatusType.Active, game.Status);
        Assert.NotEqual(default, game.CreatedAt);
        Assert.Null(game.UpdatedAt);
    }

    [Fact(DisplayName = "Validando atualização de jogo com sucesso")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Update_Success()
    {
        var game = CreateValidGame();

        game.Update(
            " Sonic 2 ",
            " Novo jogo ",
            250,
            " Plataforma ");

        Assert.Equal("Sonic 2", game.Title);
        Assert.Equal("Novo jogo", game.Description);
        Assert.Equal(250, game.Price);
        Assert.Equal("Plataforma", game.Category);
        Assert.NotNull(game.UpdatedAt);
    }

    [Fact(DisplayName = "Validando atualização de jogo inativo")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Update_Inactive()
    {
        var game = CreateValidGame();

        game.Inactivate();

        var result = Assert.Throws<DomainException>(() =>
            game.Update(
                "Sonic 2",
                "Novo jogo",
                250,
                "Plataforma"));

        Assert.Equal("O jogo está inativo.", result.Message);
    }

    [Fact(DisplayName = "Validando inativação de jogo")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Inactivate_Success()
    {
        var game = CreateValidGame();

        game.Inactivate();

        Assert.Equal(StatusType.Inactive, game.Status);
        Assert.NotNull(game.UpdatedAt);
    }

    [Fact(DisplayName = "Validando ativação de jogo")]
    [Trait("Categoria", "Validando Jogo")]
    public void Game_Activate_Success()
    {
        var game = CreateValidGame();

        game.Inactivate();
        game.Activate();

        Assert.Equal(StatusType.Active, game.Status);
        Assert.NotNull(game.UpdatedAt);
    }

    private static Game CreateValidGame()
    {
        return Game.Create(
            "Sonic",
            "Jogo de aventura",
            199,
            "Aventura");
    }
}