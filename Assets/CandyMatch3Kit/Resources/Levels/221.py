import os
import shutil
import json

def update_json_id(file_path, new_id):
    with open(file_path, 'r', encoding='utf-8') as file:
        lines = file.readlines()

    # JSON dosyasını satır satır okuyarak ikinci satırı güncelle
    for i, line in enumerate(lines):
        if '"id":' in line:
            lines[i] = f'  "id": {new_id},\n'
            break

    with open(file_path, 'w', encoding='utf-8') as file:
        file.writelines(lines)

def duplicate_and_rename_files(folder_path):
    # Klasördeki dosyaları listele
    files = os.listdir(folder_path)
    
    # Sadece dosyaları filtrele (.meta dosyaları hariç)
    files = [f for f in files if os.path.isfile(os.path.join(folder_path, f)) and not f.endswith('.meta')]
    
    # Dosyaları alfabetik olarak sırala
    files.sort()

    # Dosyaları sırayla kopyalayıp yeniden adlandır
    for idx, file_name in enumerate(files, start=110):
        # Eski dosya yolunu ve yeni dosya yolunu oluştur
        old_file_path = os.path.join(folder_path, file_name)
        new_file_name = f"{idx}{os.path.splitext(file_name)[1]}"
        new_file_path = os.path.join(folder_path, new_file_name)
        
        # Dosyayı kopyala
        shutil.copy2(old_file_path, new_file_path)
        
        print(f"'{old_file_path}' kopyalandı ve '{new_file_path}' olarak yeniden adlandırıldı.")
        
        # JSON dosyasıysa, id değerini güncelle
        if new_file_name.endswith('.json'):
            update_json_id(new_file_path, idx)

# Kullanım
if __name__ == "__main__":
    folder_path = "/Users/faruk/Desktop/SweetTTCoin/SweetTTCoin/SweetTTCoin/Assets/CandyMatch3Kit/Resources/Levels"
    duplicate_and_rename_files(folder_path)
