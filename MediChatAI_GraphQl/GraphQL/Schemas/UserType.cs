using MediChatAI_GraphQl.Shared.DTOs;
using MediChatAI_GraphQl.Features.Admin.DTOs;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Features.Authentication.DTOs;
using MediChatAI_GraphQl.Features.Notifications.DTOs;

namespace MediChatAI_GraphQl.GraphQL.Schemas;

public class UserType : ObjectType<UserInfo>
{
    protected override void Configure(IObjectTypeDescriptor<UserInfo> descriptor)
    {
        descriptor.Field(u => u.Id).Description("User's unique identifier");
        descriptor.Field(u => u.Email).Description("User's email address");
        descriptor.Field(u => u.FirstName).Description("User's first name");
        descriptor.Field(u => u.LastName).Description("User's last name");
        descriptor.Field(u => u.Role).Description("User's role (Admin, Doctor, Patient)");
        descriptor.Field(u => u.EmailConfirmed).Description("Whether the user's email is verified");
    }
}