# Permission & Audit Matrix — Platform / Alokasi Nomor Bisnis

| Field | Nilai |
| --- | --- |
| Contract version | `v1` — ✅ **`approved`** `2026-09-09` |
| `last_changed_in` | `v1` |
| Owner | Pemilik kontrak engineering backend — `Andry` |
| Traceability | `DEC-PLT-005`, `INV-PLT-001`, `QBE-CODE-002` |

---

## 1. Butir hak akses

Satu Resource, satu Action. Modul ini memang sesederhana itu dari sisi kewenangan, karena
kemampuan intinya tidak dipaparkan kepada pengguna sama sekali.

| Resource | Action | `AccessType` | Kegunaan |
| --- | --- | --- | --- |
| `NumberSeries` | `Read` | `Read` | Melihat keadaan deret nomor pada layar pemantauan |

Pasangan `[AccessPermission("NumberSeries", "Read")]` menjaga keempat endpoint pada
`api-contract.md`.

**Tidak ada butir `Create`, `Update`, maupun `Delete`**, dan itu bukan kelalaian: tidak ada satu
pun endpoint yang menulis. Mendaftarkan butir yang tidak menjaga apa pun akan menghasilkan baris
yatim yang dapat dicentang admin tetapi tidak berpengaruh.

---

## 2. Siapa yang wajar memegangnya

| Peran | Butir | Alasan |
| --- | --- | --- |
| Administrator sistem | `NumberSeries : Read` | Memeriksa keadaan deret saat menelusuri keluhan nomor |
| Pengembang / pemelihara | `NumberSeries : Read` | Memastikan deret berjalan sebagaimana mestinya |
| Peran klinis dan administratif harian | **Tidak** | Tidak ada pekerjaan sehari-hari yang menuntut membaca pencacah |

Pemberiannya tetap keputusan admin lewat layar Pengaturan → Manajemen Role → Akses Role, bukan
ditanam di kode.

---

## 3. Kewenangan mengalokasikan nomor

**Alokasi tidak dijaga butir hak akses, dan itu disengaja.**

Alasannya: alokasi bukan tindakan pengguna. Ia langkah di dalam pekerjaan yang sudah dijaga butir
hak akses miliknya sendiri. Ketika petugas menekan Simpan pada layar order darah, yang menjaganya
adalah `BloodOrder : Create`. Nomor terbit sebagai akibat, bukan sebagai tindakan terpisah.

| Yang salah | Kenapa |
| --- | --- |
| Membuat butir `NumberSeries : Allocate` | Butir itu akan menjadi kewenangan kedua yang harus diberikan bersamaan dengan setiap butir "buat catatan" mana pun. Admin yang lupa memberikannya membuat petugas tidak dapat menyimpan apa pun, dengan pesan yang tidak menyebut sebabnya |
| Memaparkan alokasi lewat HTTP lalu menjaganya | Melanggar `INV-PLT-001` — lihat `api-contract.md` §1 |

---

## 4. Audit

| Yang tersimpan | Di mana | Kapan |
| --- | --- | --- |
| Pelaku alokasi terakhir | `NumNumberSeries.CreateBy` / `UpdateBy` | Setiap alokasi |
| Waktu alokasi terakhir | `NumNumberSeries.LastAllocatedAt` | Setiap alokasi |
| Nilai yang terbit | `NumNumberSeries.CurrentValue` | Setiap alokasi |

**Batas yang jujur.** Baris deret hanya menyimpan **alokasi terakhir**, bukan riwayat setiap
nomor yang pernah terbit. Menelusuri "siapa yang menerbitkan nomor ke-57" karena itu **tidak
dapat** dijawab modul ini; jawabannya ada pada catatan yang menempeli nomor tersebut — order,
invoice, atau tindakan — yang memang menyimpan pelaku dan waktunya sendiri.

Membuat tabel riwayat per nomor dipertimbangkan dan **ditolak**: ia menggandakan jumlah baris
tulis pada jalur terpanas sistem, untuk menjawab pertanyaan yang sudah terjawab di tempat lain.

---

## 5. Privasi

| Kolom | Sensitif | Boleh masuk log |
| --- | :---: | :---: |
| `SequenceKey`, `ScopeKey`, `ResetPolicy` | Tidak | Ya |
| `CurrentValue`, `LastAllocatedAt` | Tidak | Ya |
| `CreateBy`, `UpdateBy` | Tidak | Ya — identitas pelaku, bukan data pribadi pasien |

**Nol kolom sensitif pada modul ini.** Nomor bisnis bukan data pribadi, dan modul ini tidak
pernah menyentuh data pasien.
