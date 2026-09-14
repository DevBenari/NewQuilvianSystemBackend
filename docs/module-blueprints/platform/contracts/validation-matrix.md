# Validation Matrix — Platform / Alokasi Nomor Bisnis

| Field | Nilai |
| --- | --- |
| Contract version | `v1` — ✅ **`approved`** `2026-09-09` |
| `last_changed_in` | `v1` |
| Owner | Pemilik kontrak engineering backend — `Andry` |
| Traceability | `DEC-PLT-004`, `DEC-PLT-005`, `INV-PLT-001`..`004`, `QBE-CODE-003`, `QBE-CODE-006` |

---

## 1. Validasi parameter alokasi

Kegagalan di sini dikembalikan sebagai **exception kepada service pemanggil**, bukan sebagai
response HTTP — alokasi bukan endpoint. Modul pemanggillah yang menerjemahkannya menjadi pesan
bagi petugas.

| Kode | Kapan | Pesan | Akibat |
| --- | --- | --- | --- |
| `VAL-PLT-001` | Penanda deret kosong | "Penanda deret nomor wajib diisi." | Alokasi ditolak; nol nomor terbit |
| `VAL-PLT-002` | Awalan kosong, atau lebih dari 15 karakter | "Awalan nomor wajib diisi dan maksimal 15 karakter." | Alokasi ditolak |
| `VAL-PLT-003` | Kebijakan pengulangan di luar empat nilai sah | "Kebijakan pengulangan nomor harus NEVER, YEARLY, MONTHLY, atau DAILY." | Alokasi ditolak |
| `VAL-PLT-004` | Jumlah digit di luar rentang 4–12 | "Jumlah digit nomor harus antara 4 dan 12." | Alokasi ditolak |
| `VAL-PLT-005` | Pelaku tidak dikenali | "Petugas pelaku tidak dikenali." | Alokasi ditolak; audit tidak boleh kosong |

**Kenapa alokasi ditolak, bukan dipaksakan dengan nilai bawaan.** Awalan dan panjang nomor adalah
milik modul (`DEC-PLT-005`). Menambal parameter yang salah dengan nilai bawaan platform berarti
menerbitkan nomor berbentuk lain dari yang dirancang modulnya, dan bentuk itu terlanjur menempel
permanen pada catatan.

---

## 2. Validasi keadaan penyimpanan

| Kode | Kapan | Pesan | Akibat |
| --- | --- | --- | --- |
| `VAL-PLT-006` | Pencacah melampaui batas tipe bilangan | "Deret nomor ini sudah mencapai batas dan tidak dapat dinaikkan lagi." | Alokasi ditolak. Terkait `OQ-PLT-005` yang masih terbuka |
| `VAL-PLT-007` | Jumlah digit tidak cukup menampung nilai pencacah | "Nilai deret melampaui jumlah digit yang dikonfigurasi." | Alokasi ditolak — lihat catatan di bawah |

**`VAL-PLT-007` adalah pelajaran dari `FACT-PLT-009`.** `LegalEntityController` memakai tiga digit,
sehingga deretnya habis diam-diam setelah `999` dan nomor berikutnya menjadi `1000` yang tidak
lagi sesuai bentuknya. Menolaknya secara tegas lebih baik daripada menerbitkan nomor yang bentuknya
menyimpang, karena bentuk yang menyimpang baru ketahuan jauh di belakang.

**Contoh berangka.** Deret dikonfigurasi 4 digit dan pencacah mencapai `9999`. Alokasi berikutnya
akan bernilai `10000`, yang tidak muat. Alokasi itu **ditolak** dengan `VAL-PLT-007`, bukan
diterbitkan sebagai `10000`. Yang menyelesaikannya adalah keputusan pemilik lewat `OQ-PLT-005`.

---

## 3. Aturan yang ditegakkan struktur, bukan pemeriksaan

Sebagian aturan terkuat modul ini tidak berupa kode validasi, melainkan ketiadaan jalur.

| Aturan | Cara ditegakkan |
| --- | --- |
| Pencacah tidak pernah turun | **Nol** method yang menurunkannya. Nol endpoint yang menyentuhnya |
| Nomor tidak pernah diterbitkan lewat HTTP | **Nol** endpoint alokasi. Controller modul ini hanya membaca |
| Deret tidak pernah dihapus | **Nol** endpoint hapus; `IsDelete` tidak pernah disetel kode mana pun |
| Nomor tidak dialokasikan controller | Alokator adalah service dan hanya di-`inject` ke service lain (`QBE-CODE-002`) |
| Alokasi tidak memakai `Count+1`/`Max+1` | Nilai diambil dari pencacah tersimpan, bukan dihitung dari data (`QBE-CODE-003`) |
| Dua permintaan bersamaan tidak menghasilkan nomor kembar | `pg_advisory_xact_lock` **ditambah** index unik `(SequenceKey, ScopeKey)` sebagai penjaga terakhir |

---

## 4. Yang sengaja tidak divalidasi

| Tidak divalidasi | Alasan |
| --- | --- |
| Apakah nomor yang terbit benar-benar dipakai | Alokator tidak tahu, dan tidak boleh tahu, nasib catatan pemanggilnya. Nomor hangus adalah keadaan sah (`DEC-PLT-008`) |
| Apakah deret berlubang | Lubang adalah keadaan sah (`INV-PLT-002`). Memeriksanya mengundang niat merapikannya |
| Keunikan nomor jadi lintas deret | Dua deret berbeda boleh menghasilkan angka urut yang sama; yang membedakan adalah awalannya. Keunikan nomor bisnis dijaga index unik pada kolom milik modul masing-masing (`QBE-CODE-004`) |
| Format awalan milik modul | Milik modul (`DEC-PLT-005`). Platform hanya membatasi panjangnya |
