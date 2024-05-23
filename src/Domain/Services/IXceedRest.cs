using Domain.Models;
using Domain.Models.Messages;
using Refit;

namespace Domain.Services;

[Headers([
    "Connection:Keep-Alive",
    "Content-Type:application/json; charset=utf-8"
])]
public interface IXceedRest
{
    [Post("/addevent")]
    public Task<XceedMessageResponse> SendAddEvent([Query]XceedParams xceedParams, [Body] string body);
}
