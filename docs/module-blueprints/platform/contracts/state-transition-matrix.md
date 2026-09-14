# State Transition Matrix — Platform / Alokasi Nomor Bisnis

| Field | Nilai |
| --- | --- |
| Contract version | `v1` — ✅ **`approved`** `2026-09-09` |
| `last_changed_in` | `v1` |
| Owner | Pemilik kontrak engineering backend — `Andry` |
| Traceability | `DEC-PLT-002`, `DEC-PLT-008`, `INV-PLT-001`, `INV-PLT-002` |

---

## 1. Modul ini tidak punya mesin status

Dinyatakan tegas supaya tidak dicari-cari di kemudian hari: **`NumNumberSeries` tidak memiliki
kolom status, dan tidak memiliki mesin status.** Ia pencacah — satu bilangan yang hanya bergerak
naik.

Karena itu dokumen ini tidak memuat tabel perpindahan status seperti modul bisnis pada umumnya.
Yang menggantikannya adalah **matriks perpindahan nilai pencacah**, karena di situlah seluruh
aturan yang mengikat berada.

---

## 2. Perpindahan nilai pencacah

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| Baris belum ada | Alokasi pertama pada deret + periode | Baris lahir, `CurrentValue = 1` | Service modul pemilik catatan | Parameter deret sah | `VAL-PLT-001`..`004` |
| `CurrentValue = n` | Alokasi berikutnya | `CurrentValue = n + 1` | Service modul pemilik catatan | Giliran didapat pada antrean deret | — |
| `CurrentValue = n` | **Menurunkan nilai** | — | **Tidak seorang pun** | — | Melanggar `INV-PLT-001`; nol jalur kode yang menyediakannya |
| `CurrentValue = n` | **Menyetel ulang ke nol** | — | **Tidak seorang pun** | — | Melanggar `INV-PLT-001` dan `INV-PLT-002` |
| Baris ada | **Menghapus baris** | — | **Tidak seorang pun** | — | Alokasi berikutnya mengulang dari satu; nomor kembar terbit |

Tiga baris terakhir sengaja ditulis walaupun tidak ada jalur kodenya. Ia menyatakan bahwa
ketiadaan itu **disengaja**, bukan belum sempat dibuat.

---

## 3. Perpindahan nasib nomor yang sudah terbit

Inilah yang benar-benar diputuskan `DEC-PLT-008`.

| Keadaan nomor | Kejadian | Nasib nomor | Nasib pencacah |
| --- | --- | --- | --- |
| Baru terbit, belum menempel | Pekerjaan modul **berhasil** disimpan | Menempel permanen pada satu catatan | Tetap naik |
| Baru terbit, belum menempel | Pekerjaan modul **dibatalkan** | **Hangus selamanya** — tidak pernah diterbitkan lagi | **Tetap naik** |
| Sudah menempel | Catatannya dibatalkan bisnis | **Tetap milik catatan itu** | Tidak bergerak |
| Sudah menempel | Catatannya dihapus | **Tetap hangus**, tidak dikembalikan ke deret | Tidak bergerak |

**Baris kedua adalah inti perubahan terhadap mesin yang ada sekarang.** Mesin Billing melakukan
kebalikannya — pencacah ikut turun, nomor terbit lagi (`CONF-PLT-002`).

---

## 4. Perpindahan yang dilarang

| Perpindahan | Kenapa dilarang |
| --- | --- |
| Nomor hangus → dipakai lagi | Petugas mungkin sudah melihat, mencatat, atau menyebutkan nomor itu sebelum pekerjaan batal. Menerbitkannya lagi membuat satu nomor menunjuk dua hal pada ingatan orang, walau di database hanya satu (`INV-PLT-001`) |
| Lubang deret → diisi | Mengisi celah berarti mencari nomor yang belum terpakai lalu memberikannya. Nomor itu justru yang paling mungkin sudah beredar (`INV-PLT-002`) |
| Satu deret dilayani dua mekanisme bersamaan | Dua pencacah pada deret yang sama pasti menerbitkan nomor kembar (`INV-PLT-003`) |
| Deret diulang dari awal saat ganti tahun atau ganti fasilitas | `INV-PLT-004`. Untuk deret baru, `DEC-PLT-004` menetapkan `ResetPolicy = NEVER` |

---

## 5. Catatan masa peralihan

Selama `PLT-SLICE-02` belum dikerjakan, dua mekanisme hidup berdampingan:

| Deret | Dilayani | Perilaku saat pemanggil batal |
| --- | --- | --- |
| Empat deret Billing (`BILLING_*`) | Mesin lama | Nomor **dipakai ulang** |
| Seluruh deret baru | Mesin platform | Nomor **hangus** |

Ini **tidak** melanggar `INV-PLT-003`, karena invariant itu melarang satu **deret** dilayani dua
mekanisme — bukan melarang dua mekanisme ada di satu sistem. Perbedaan perilaku ini berakhir
ketika `PLT-SLICE-02` memindahkan keempat deret Billing, dan itu menuntut `DEC-PLT-006` dijawab.
