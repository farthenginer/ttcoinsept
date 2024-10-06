import os
import shutil
import random

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

def get_next_available_id(folder_path, start_id):
    # Klasördeki dosyaları listele
    files = os.listdir(folder_path)
    
    # Sadece dosyaları filtrele (.meta dosyaları hariç)
    files = [f for f in files if os.path.isfile(os.path.join(folder_path, f)) and not f.endswith('.meta')]
    
    # Mevcut dosya numaralarını topla
    existing_ids = set()
    for file_name in files:
        base_name, ext = os.path.splitext(file_name)
        if base_name.isdigit():
            existing_ids.add(int(base_name))
    
    # Yeni numarayı bul
    while start_id in existing_ids:
        start_id += 1
    return start_id

def duplicate_and_rename_files(folder_path, start_id=220, num_files=80):
    # Klasördeki dosyaları listele
    files = os.listdir(folder_path)
    
    # Sadece dosyaları filtrele (.meta dosyaları hariç)
    files = [f for f in files if os.path.isfile(os.path.join(folder_path, f)) and not f.endswith('.meta')]
    
    # Dosyaları alfabetik olarak sırala
    files.sort()

    # Rastgele 80 dosya seç
    selected_files = random.sample(files, num_files)

    # Mevcut ID'den devam et
    current_id = get_next_available_id(folder_path, start_id)
    
    # Seçilen dosyaları sırayla kopyalayıp yeniden adlandır
    for file_name in selected_files:
        # Eski dosya yolunu ve yeni dosya yolunu oluştur
        old_file_path = os.path.join(folder_path, file_name)
        new_file_name = f"{current_id}{os.path.splitext(file_name)[1]}"
        new_file_path = os.path.join(folder_path, new_file_name)

        # Dosyayı kopyala
        if not os.path.exists(new_file_path):  # Dosya var mı kontrol et
            shutil.copy2(old_file_path, new_file_path)
            print(f"'{old_file_path}' kopyalandı ve '{new_file_path}' olarak yeniden adlandırıldı.")
        
            # JSON dosyasıysa, id değerini güncelle
            if new_file_name.endswith('.json'):
                update_json_id(new_file_path, current_id)
            
            current_id += 1  # ID'yi artır
        else:
            print(f"'{new_file_path}' zaten mevcut, atlanıyor.")

# Kullanım
if __name__ == "__main__":
    folder_path = "/Users/faruk/Desktop/SweetTTCoin/SweetTTCoin/SweetTTCoin/Assets/CandyMatch3Kit/Resources/Levels"
    duplicate_and_rename_files(folder_path)
