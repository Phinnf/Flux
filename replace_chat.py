import os
import re

file_path = r'G:\BTEC\Ky6\DATN\Flux\Flux\Components\Pages\Chat.razor'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace FluentStack
content = re.sub(r'<FluentStack[^>]*>', '<div class="flex items-center gap-2">', content)
content = content.replace('</FluentStack>', '</div>')

# Replace FluentSpacer
content = re.sub(r'<FluentSpacer\s*/>', '<div class="flex-1"></div>', content)

# Replace FluentButton
content = re.sub(r'<FluentButton[^>]*Appearance="Appearance.Stealth"[^>]*OnClick="([^"]*)"[^>]*>([^<]*)</FluentButton>', r'<button class="px-4 py-2 text-gray-300 hover:text-white transition-colors" @onclick="\1">\2</button>', content)
content = re.sub(r'<FluentButton[^>]*Appearance="Appearance.Stealth"[^>]*OnClick="([^"]*)"[^>]*/>', r'<button class="p-2 text-gray-400 hover:text-white transition-colors" @onclick="\1"></button>', content)

content = re.sub(r'<FluentButton[^>]*OnClick="([^"]*)"[^>]*>([^<]*)</FluentButton>', r'<button class="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors" @onclick="\1">\2</button>', content)
content = re.sub(r'<FluentButton[^>]*OnClick="([^"]*)"[^>]*/>', r'<button class="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors" @onclick="\1"></button>', content)

content = re.sub(r'<FluentButton([^>]*)>([^<]*)</FluentButton>', r'<button class="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors" \1>\2</button>', content)
content = re.sub(r'<FluentButton([^>]*)/>', r'<button class="p-2 text-gray-400 hover:text-white transition-colors" \1></button>', content)

# Replace FluentIcon
content = re.sub(r'<FluentIcon[^>]*/>', '<div class="w-5 h-5 bg-gray-500 rounded-full"></div>', content)

# Replace FluentLabel
content = re.sub(r'<FluentLabel[^>]*>([^<]*)</FluentLabel>', r'<span class="text-white font-medium">\1</span>', content)

# Replace FluentProgressRing
content = re.sub(r'<FluentProgressRing[^>]*/>', '<div class="w-5 h-5 border-2 border-blue-500 border-t-transparent rounded-full animate-spin"></div>', content)

# Replace FluentTextArea
content = re.sub(r'<FluentTextArea\s+@ref="_textAreaRef"\s+Value="@_newMessageContent"\s+ValueChanged="@\(EventCallback\.Factory\.Create<string>\(this, OnChatInputChanged\)\)"\s+ValueExpression="@\(\(\) => _newMessageContent\)"\s+Immediate="true"\s+Placeholder="([^"]*)"\s+title="([^"]*)"\s+Class="([^"]*)"\s+Rows="([^"]*)"\s*/>', r'<textarea @ref="_textAreaRef" :value="_newMessageContent" @input="@(e => OnChatInputChanged((string)e.Value))" placeholder="\1" title="\2" class="w-full px-4 py-2 bg-gray-700 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500 \3" rows="\4"></textarea>', content)
content = re.sub(r'<FluentTextArea\s+@bind-Value="_editMessageContent"\s+Class="([^"]*)"\s+Rows="([^"]*)"\s*/>', r'<textarea @bind="_editMessageContent" class="w-full px-4 py-2 bg-gray-700 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500 \1" rows="\2"></textarea>', content)
content = re.sub(r'<FluentTextArea\s+@ref="_threadTextAreaRef"\s+Value="@_newThreadMessageContent"\s+ValueChanged="@\(EventCallback\.Factory\.Create<string>\(this, OnThreadInputChanged\)\)"\s+ValueExpression="@\(\(\) => _newThreadMessageContent\)"\s+Immediate="true"\s+Placeholder="([^"]*)"\s+Class="([^"]*)"\s+Rows="([^"]*)"\s*/>', r'<textarea @ref="_threadTextAreaRef" :value="_newThreadMessageContent" @input="@(e => OnThreadInputChanged((string)e.Value))" placeholder="\1" class="w-full px-4 py-2 bg-gray-700 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500 \2" rows="\3"></textarea>', content)

# Replace FluentPersona
content = re.sub(r'<FluentPersona\s+Status="[^"]*"\s+ImageSize="([^"]*)"\s+Image="([^"]*)"\s+Initials="([^"]*)"\s+Title="([^"]*)"\s*/>', r'''<div class="rounded-full bg-blue-100 flex items-center justify-center text-blue-600 font-bold overflow-hidden" style="width: \1; height: \1;" title="\4">
    @if (!string.IsNullOrEmpty(\2)) { <img src="\2" class="w-full h-full object-cover" /> } else { \3 }
</div>''', content)

content = re.sub(r'<FluentPersona\s+Status="[^"]*"\s+ImageSize="([^"]*)"\s+Image="([^"]*)"\s+Initials="([^"]*)"\s*/>', r'''<div class="rounded-full bg-blue-100 flex items-center justify-center text-blue-600 font-bold overflow-hidden" style="width: \1; height: \1;">
    @if (!string.IsNullOrEmpty(\2)) { <img src="\2" class="w-full h-full object-cover" /> } else { \3 }
</div>''', content)

content = re.sub(r'<FluentPersona\s+ImageSize="([^"]*)"\s+Initials="([^"]*)"\s*/>', r'''<div class="rounded-full bg-blue-100 flex items-center justify-center text-blue-600 font-bold" style="width: \1; height: \1;">\2</div>''', content)

# Replace FluentDivider
content = re.sub(r'<FluentDivider\s*/>', '<div class="h-px w-full bg-gray-700 my-1"></div>', content)
content = re.sub(r'<FluentDivider\s+Vertical="true"\s+style="([^"]*)"\s*/>', r'<div class="w-px bg-gray-700 mx-2" style="\1"></div>', content)

# Replace FluentMenuItem
content = re.sub(r'<FluentMenuItem[^>]*OnClick="([^"]*)"[^>]*>(.*?)</FluentMenuItem>', r'<button class="w-full text-left px-4 py-2 hover:bg-gray-700 text-white flex items-center gap-2" @onclick="\1">\2</button>', content, flags=re.DOTALL)

# Replace FluentDialog
content = re.sub(r'<FluentDialog\s+Hidden="!_showDeleteDialog"\s+Modal="true"\s+TrapFocus="true"\s+@onclick:stopPropagation>', r'''@if (_showDeleteDialog)
{
<div class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50" @onclick="() => _showDeleteDialog = false">
    <div @onclick:stopPropagation>''', content)
content = content.replace('</FluentDialog>', '</div></div>\n}')

# Remove IToastService injection
content = re.sub(r'@inject IToastService ToastService\n', '', content)
content = re.sub(r'ToastService\.ShowError\(([^)]*)\);', r'Console.WriteLine("Error: " + \1);', content)
content = re.sub(r'ToastService\.ShowSuccess\(([^)]*)\);', r'Console.WriteLine("Success: " + \1);', content)
content = re.sub(r'ToastService\.ShowInfo\(([^)]*)\);', r'Console.WriteLine("Info: " + \1);', content)
content = re.sub(r'ToastService\.ShowWarning\(([^)]*)\);', r'Console.WriteLine("Warning: " + \1);', content)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)
