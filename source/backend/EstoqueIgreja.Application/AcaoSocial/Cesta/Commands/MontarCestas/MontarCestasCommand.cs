using MediatR;

namespace EstoqueIgreja.Application.AcaoSocial.Cesta.Commands.MontarCestas;

public record MontarCestasCommand(int Quantidade) : IRequest<CestasMontadasResult>;

public record CestasMontadasResult(int Montadas, int CestasProntas);
