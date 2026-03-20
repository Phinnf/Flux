import re

file_path = 'Flux/Components/Pages/Chat.razor'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Find the start of the @code block
match = re.search(r'^@code\s*\{', content, re.MULTILINE)
if match:
    start_index = match.start()
    razor_content = content[:start_index]
    code_content = content[start_index + len('@code {'):].strip()
    
    # Remove the last closing brace of the @code block
    if code_content.endswith('}'):
        code_content = code_content[:-1]
    
    cs_content = f'''using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Flux.Features.Messages.EditMessage;
using Flux.Features.Messages.SendMessage;
using Flux.Features.Messages.GetMessages;
using Flux.Infrastructure.Client;

namespace Flux.Components.Pages;

public partial class Chat : ComponentBase, IAsyncDisposable
{{
    [Inject] private FluxClientService FluxService {{ get; set; }} = default!;
    [Inject] private WorkspaceStateService StateService {{ get; set; }} = default!;
    [Inject] private NavigationManager Navigation {{ get; set; }} = default!;
    [Inject] private IJSRuntime JS {{ get; set; }} = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider {{ get; set; }} = default!;
    [Inject] private Flux.Infrastructure.Services.IToastService ToastService {{ get; set; }} = default!;

    {code_content}
}}
'''
    
    with open('Flux/Components/Pages/Chat.razor', 'w', encoding='utf-8') as f:
        f.write(razor_content)
        
    with open('Flux/Components/Pages/Chat.razor.cs', 'w', encoding='utf-8') as f:
        f.write(cs_content)
    
    print('Extraction successful!')
else:
    print('@code block not found.')
