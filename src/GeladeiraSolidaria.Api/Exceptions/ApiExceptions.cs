namespace GeladeiraSolidaria.Api.Exceptions;

/// <summary>Registro que nao existe. Vira 404.</summary>
public class NaoEncontradoException(string mensagem) : Exception(mensagem);

/// <summary>Conflito com o que ja esta no banco (nome repetido, exclusao com historico). Vira 409.</summary>
public class ConflitoException(string mensagem) : Exception(mensagem);

/// <summary>Os dados sao validos, mas uma regra de negocio impede a operacao. Vira 422.</summary>
public class RegraNegocioException(string mensagem) : Exception(mensagem);
