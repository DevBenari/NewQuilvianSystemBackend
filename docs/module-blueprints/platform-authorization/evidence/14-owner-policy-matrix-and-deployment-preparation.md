# Matriks Hak Akses yang Disetujui Pemilik dan Persiapan Penerapan

> **Mode:** persiapan berbasis bukti. Tidak ada database yang disentuh, tidak ada
> `AccessMenuSeeder` yang dijalankan, tidak ada aplikasi yang dinyalakan, tidak ada skrip yang
> dieksekusi, tidak ada Integration yang di-fetch atau di-merge, tidak ada commit/push.

| Field | Nilai |
|---|---|
| Jenis dokumen | **Persiapan penerapan** — memuat keputusan pemilik dan urutan yang mengikat |
| Sebab | `BE-SEC-012` dan `BE-SEC-013` selesai di source; penerapannya menuntut matriks hak yang disetujui |
| Kandidat beku | `4ff7b9987cda7d11df22080ff1b2f849bbc97319` + perubahan `BE-SEC-012` dan `BE-SEC-013` yang belum di-commit |
| Baseline source baru | **1.300 action / 340 resource / 48 modul** |
| Tanggal | 16 September 2026 |
| Status | **Siap diterapkan** setelah operator menjalankan urutan bagian E |

---

## A. Keputusan pemilik sistem

Untuk **master data**, otorisasi mengikuti **kepemilikan departemen**.

| Peran | Read | Create | Update | Delete |
|---|:---:|:---:|:---:|:---:|
| Manajer departemen pemilik | ✅ | ✅ | ✅ | ✅ |
| Staff departemen pemilik | ✅ | ✅ | ❌ | ❌ |

Jadwal kerja, shift, grup shift, pola shift, kalender kerja, dan penugasan jadwal kerja adalah
master data milik **Human Resource**.

**Finance × Manajer Finance tidak boleh mempertahankan `WorkSchedule.Update` / `WorkSchedule.Delete`
hanya karena policy warisan kebetulan ada.** Tidak ada izin Finance lain yang boleh disimpulkan dari
keputusan ini.

---

## B. Matriks yang disetujui

Enam resource, empat action.

### B.1 Human Resource × Manajer HR

| Resource | Read | Create | Update | Delete |
|---|:---:|:---:|:---:|:---:|
| `WorkSchedule` | ✅ | ✅ | ✅ | ✅ |
| `Shift` | ✅ | ✅ | ✅ | ✅ |
| `ShiftGroup` | ✅ | ✅ | ✅ | ✅ |
| `ShiftPattern` | ✅ | ✅ | ✅ | ✅ |
| `WorkCalendar` | ✅ | ✅ | ✅ | ✅ |
| `WorkScheduleAssignment` | ✅ | ✅ | ✅ | ✅ |

**24 pemberian hak.**

### B.2 Human Resource × posisi staff

| Resource | Read | Create | Update | Delete |
|---|:---:|:---:|:---:|:---:|
| keenam resource di atas | ✅ | ✅ | ❌ | ❌ |

**12 pemberian hak per posisi staff.**

> **Posisi staff belum dapat disebut namanya.** Daftar posisi pada Departemen Human Resource tidak
> tersedia sebagai bukti, dan menebaknya berarti mengarang penerima hak. Operator menjalankan
> bagian 2.1 [`be-sec-014-current-holders-readonly-dbeaver.sql`](../../../../Migrations/scripts/be-sec-014-current-holders-readonly-dbeaver.sql),
> menunjukkan keluarannya kepada pemilik sistem, lalu menuliskan nama yang **disetujui** ke bagian
> 3.2 skrip pemberian hak. Dibiarkan kosong, hanya Manajer HR yang dilayani — kurang memberi jauh
> lebih aman daripada mengarang.

### B.3 Finance

| Resource | Read | Create | Update | Delete |
|---|:---:|:---:|:---:|:---:|
| `WorkSchedule` | — tidak diubah — | — tidak diubah — | ❌ **dicabut** | ❌ **dicabut** |
| lima resource lain | — tidak diubah — | — tidak diubah — | ❌ tidak diberikan | ❌ tidak diberikan |

Yang dicabut **hanya** `WorkSchedule.Update` dan `WorkSchedule.Delete` milik Finance × Manajer
Finance. `Read` dan `Create` milik siapa pun tidak disentuh, dan tidak ada izin Finance lain yang
disimpulkan.

---

## C. Klasifikasi setiap identitas

Empat golongan yang berbeda, dan perlakuannya tidak sama.

### C.1 Identitas yang sudah ada sebelum `BE-SEC-012`

| Resource | Action | Keterangan |
|---|---|---|
| `WorkSchedule`, `Shift`, `ShiftGroup`, `ShiftPattern`, `WorkCalendar` | `Read`, `Create` | Sudah terdaftar dan ditegakkan sejak awal |

Pemegangnya hari ini **tidak diketahui dari bukti yang tersedia** dan sengaja tidak ditebak.
Bagian 1.2 skrip pembacaan menjawabnya sebelum pemberian hak dijalankan.

### C.2 Hak tertidur — wajib dicabut **sebelum** seeder

| Departemen × Posisi | Identitas | Policy | Perlakuan |
|---|---|---:|---|
| Finance × Manajer Finance | `WorkSchedule.Update` | 1 | **Dinonaktifkan sebelum seeder** |
| Finance × Manajer Finance | `WorkSchedule.Delete` | 1 | **Dinonaktifkan sebelum seeder** |
| Human Resource × Manajer HR | `WorkSchedule.Update` | 1 | **Dibiarkan** — sesuai matriks |
| Human Resource × Manajer HR | `WorkSchedule.Delete` | 1 | **Dibiarkan** — sesuai matriks |

Keempat baris ini hari ini tidak memberi hak apa pun karena identitasnya tertutup. Seeder akan
**mengaktifkan ulang** identitasnya, dan saat itu juga keempatnya menjadi hak yang hidup. Dua baris
Finance karena itu wajib dinonaktifkan lebih dulu; dua baris HR justru memang diinginkan dan tidak
perlu disisipkan ulang.

### C.3 Identitas yang benar-benar baru

| Resource | Action baru | Task | Pemegang awal |
|---|---|---|---|
| `WorkSchedule`, `Shift`, `ShiftGroup`, `ShiftPattern`, `WorkCalendar` | `Update`, `Delete` | `BE-SEC-012` | **Nol** |
| `WorkScheduleAssignment` | `Read`, `Create`, `Update`, `Delete` | `BE-SEC-013` | **Nol** |

`WorkScheduleAssignment` adalah **resource baru sepenuhnya** — identitasnya tidak pernah ada di
registry, sehingga tidak ada satu pun `SysAccessPolicy` yang dapat menunjuknya dan **tidak ada hak
tertidur** yang bisa hidup kembali. Ini perbedaan penting dari `WorkSchedule`.

### C.4 Policy yang disisipkan **sesudah** seeder

| Penerima | Jumlah |
|---|---:|
| Human Resource × Manajer HR | 24, dikurangi yang sudah ada (paling tidak 2 sudah ada: `WorkSchedule.Update`/`Delete`) |
| Human Resource × tiap posisi staff | 12 per posisi |

Skrip menyisipkan **hanya kunci alami yang belum ada**, sehingga baris `Read`/`Create` yang sudah
dipegang tidak digandakan, dan dua baris HR pada C.2 cukup dibiarkan.

---

## D. Berkas yang disiapkan

| Berkas | Sifat | Kapan dijalankan |
|---|---|---|
| [`be-sec-014-current-holders-readonly-dbeaver.sql`](../../../../Migrations/scripts/be-sec-014-current-holders-readonly-dbeaver.sql) | **Baca-saja**, tanpa transaksi | Kapan saja, termasuk sebelum jendela pemeliharaan |
| [`be-sec-014-pre-seeder-revoke-finance-workschedule.sql`](../../../../Migrations/scripts/be-sec-014-pre-seeder-revoke-finance-workschedule.sql) | Menulis, default `ROLLBACK` | **Sebelum** seeder |
| [`be-sec-014-post-seeder-hr-initial-grants.sql`](../../../../Migrations/scripts/be-sec-014-post-seeder-hr-initial-grants.sql) | Menulis, default `ROLLBACK` | **Sesudah** seeder |

Ketiganya memenuhi syarat yang sama: tidak ada GUID departemen/posisi/action yang diketik, seluruh
Id diturunkan dari kunci bisnis, UUID operator wajib diisi dan diverifikasi ada di `AspNetUsers`,
penegasan kardinalitas membatalkan transaksi bila jumlahnya berbeda, dan setiap skrip tulis
menyediakan blok rollback.

> **Catatan teknis yang mudah terlewat.** Ketiga berkas sengaja **tidak** memakai meta-command psql
> (`\set`). Alasannya dua: DBeaver tidak mengenalnya, dan psql **tidak** menginterpolasi
> `:'variabel'` di dalam blok dollar-quoted (`$$ ... $$`) — sehingga penegasan di dalam `DO` tidak
> akan pernah membaca nilai yang dimaksud dan gerbang keamanannya lolos secara semu. Parameter
> karena itu disimpan lewat `set_config(..., true)` dan dibaca `current_setting()`, yang merupakan
> SQL biasa dan berperilaku sama di kedua alat.

---

## E. Urutan penerapan yang mengikat

Urutan ini **tidak boleh dibalik**. Setiap langkah punya alasan yang dapat diperiksa.

| No | Langkah | Kenapa urutannya di sini |
|---:|---|---|
| 1 | **Backup database dikonfirmasi** | Langkah 3, 5, dan 7 mengubah data. Tanpa backup, tidak ada jalan kembali bila asumsi keliru |
| 2 | **Traffic pengguna ditutup** | Antara langkah 5 dan 7 identitas baru sudah ditegakkan endpoint tetapi belum dipegang siapa pun. Pengguna yang masuk pada jendela itu ditolak `403` |
| 3 | **Cabut hak tertidur Finance** | Wajib **sebelum** seeder. Sesudahnya, ini berubah dari mencegah hak yang belum ada menjadi mencabut hak yang sedang dipakai |
| 4 | **Konfirmasi: `Finance × Manajer Finance` = 0 baris aktif** | Bagian 3.1 skrip pencabutan. Bila belum nol, jangan lanjut |
| 5 | **Jalankan source beku SEKALI**, tanpa traffic | `AccessMenuSeeder` mendaftarkan 14 identitas baru dan mengaktifkan ulang yang tertutup |
| 6 | **Verifikasi registry = 1.300 / 340 / 48** | Bagian 3.2 skrip pembacaan. Angka lain berarti ada yang tidak terduga dan langkah 7 ditahan |
| 7 | **Terapkan pemberian hak HR** | Bagian 1 skrip pemberian hak wajib seluruhnya `siap` lebih dulu |
| 8 | **Verifikasi otorisasi HR** | Bagian 4.1 dan 4.2. Bagian 4.2 wajib nol baris |
| 9 | **Jalankan ulang dry-run `BE-SEC-003B`** | Baseline lama `1.286 / 339 / 48` sudah tidak berlaku; dry-run lama kedaluwarsa |
| 10 | **Baru Tahap 1, lalu Tahap 2** | Sesuai urutan `BE-SEC-003B` yang sudah ditetapkan |

**Tidak satu pun langkah di atas dijalankan pada task ini.**

### E.1 Jendela yang wajib tertutup

Langkah **2 sampai 8** berada dalam satu jendela pemeliharaan yang sama. Alasannya bukan drift
registry, melainkan ini: di antara langkah 5 dan 7, kemampuan HR sudah ditegakkan tetapi belum
diberikan kepada siapa pun. Bagi pengguna, layar master data kepegawaian akan tampak kosong dan
setiap tombol menolak — persis seperti gangguan. Menutup traffic membuat jendela itu tidak terlihat
pengguna.

---

## F. Baseline pemeliharaan baru

Baseline lama **`1.286 / 339 / 48` dinyatakan tidak berlaku.**

| Metrik | Baseline lama | **Baseline baru** | Sebab perubahan |
|---|---:|---:|---|
| `SysActionAccess` aktif | 1.286 | **1.300** | +10 `BE-SEC-012`, +4 `BE-SEC-013` |
| `SysControllerAccess` aktif | 339 | **340** | +1 `WorkScheduleAssignment` |
| `SysApplicationModule` aktif | 48 | **48** | tidak berubah |
| Fallback kompatibilitas | 69 | **69** | tidak berubah |
| Metadata gap | 0 | **0** | tidak berubah |
| Naked endpoint | tidak terukur | **0** | invarian baru `BE-SEC-012`, utang ditutup `BE-SEC-013` |

Setiap dokumen, skrip, atau dry-run yang masih menyebut `1.286 / 339 / 48` sebagai target
**kedaluwarsa** dan wajib dijalankan ulang terhadap baseline baru.

---

## G. Yang sengaja dibiarkan di luar scope

| Identitas | Status |
|---|---|
| `KioskScanSession.Cancel` | Terbuka — keputusan pemilik modul Registration |
| `Queue.Read`, `Queue.Update` | Terbuka — pensiun aman, migrasi policy belum dijadwalkan |
| `BillingItemCategory.*` | Terbuka — milik tim Billing, lihat `evidence/10` |

Ketiganya **tidak** disentuh skrip mana pun pada `BE-SEC-014`, dan tidak diselesaikan di sini.
