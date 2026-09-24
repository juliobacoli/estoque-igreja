namespace EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Common;

public record EstoqueSocialAlteradoResult(Guid ItemId, int QuantidadeAnterior, int QuantidadeNova);
