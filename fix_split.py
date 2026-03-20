cs_file = 'Flux/Components/Pages/Chat.razor.cs'
razor_file = 'Flux/Components/Pages/Chat.razor'

with open(cs_file, 'r', encoding='utf-8') as f:
    cs_content = f.read()

# Find where <style> starts
style_idx = cs_content.find('<style>')

if style_idx != -1:
    style_content = cs_content[style_idx:]
    # Remove trailing } from style_content which we accidentally appended in previous script
    if style_content.strip().endswith('}'):
        style_content = style_content.strip()[:-1].strip()
        
    actual_cs = cs_content[:style_idx].strip()
    
    # We still need the closing brace for the C# class!
    if not actual_cs.endswith('}'):
        actual_cs += '\n}\n'

    with open(cs_file, 'w', encoding='utf-8') as f:
        f.write(actual_cs)
        
    with open(razor_file, 'a', encoding='utf-8') as f:
        f.write('\n' + style_content + '\n')
        
    print('Fixed files successfully!')
else:
    print('Style tag not found.')
