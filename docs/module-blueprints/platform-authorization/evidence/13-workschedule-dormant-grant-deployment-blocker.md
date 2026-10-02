# Deployment Blocker — hak `WorkSchedule` yang tertidur dapat hidup kembali sendiri

> **Mode:** catatan blocker berbasis bukti source. Tidak ada database yang disentuh, tidak ada
> `AccessMenuSeeder` yang dijalankan, tidak ada `SysAccessPolicy` yang diubah.

| Field | Nilai |
|---|---|
| Jenis dokumen | **Deployment blocker** — wajib ditutup sebelum penerapan |
| Sebab | Perbaikan source `BE-SEC-012` menambah kembali identitas `WorkSchedule.Update` dan `WorkSchedule.Delete` |
| Menahan | Setiap lingkungan yang menjalankan `AccessMenuSeeder` dengan source hasil `BE-SEC-012` |
| Pemilik keputusan | Pemilik modul Human Resource |
| Tanggal | 16 September 2026 |
| Status | **TERBUKA** |

---

## 1. Ringkas — apa yang bisa terjadi tanpa disadari

Perbaikan `BE-SEC-012` menutup lubang keamanan: `PUT`, `PATCH /{id}/status`, dan `DELETE` pada
jadwal kerja tidak lagi dapat dipanggil siapa pun yang punya login.

Tetapi perbaikan itu **juga** menghidupkan kembali dua identitas yang selama ini tertutup. Di
database sudah ada empat baris `SysAccessPolicy` yang menunjuk kedua identitas itu. Baris-baris itu
saat ini tidak memberi hak apa pun karena identitasnya tertutup. Begitu identitasnya terbuka lagi,
**keempat baris itu langsung menjadi pemberian hak yang hidup — tanpa ada yang menyetujuinya.**

| Identitas | Policy efektif | Pemegang |
|---|---:|---|
| `WorkSchedule.Update` | 2 | Finance × Manajer Finance; Human Resource × Manajer HR |
| `WorkSchedule.Delete` | 2 | Finance × Manajer Finance; Human Resource × Manajer HR |

---

## 2. Buktinya di source

**Langkah 1 — hari ini hak itu mati.** Sebuah policy hanya memberi hak bila seluruh barisnya aktif
([`AccessPermissionService.cs:294-303`](../../../../Services/Security/AccessPermissionService.cs#L294-L303)):

```text
policy.IsActive && !policy.IsDelete
  && action.IsActive && !action.IsDelete
  && action.ControllerAccess.IsActive && !action.ControllerAccess.IsDelete
```

Baris `SysActionAccess` untuk `WorkSchedule.Update` dan `WorkSchedule.Delete` **tertutup**, sehingga
keempat policy itu tidak memberi apa pun.

**Langkah 2 — seeder membuka kembali baris yang sudah ada.** `AccessMenuSeeder` mencari baris
berdasarkan kunci `(ControllerAccessId, ActionName)`. Bila barisnya **sudah ada**, ia tidak membuat
baris baru melainkan menyetel ulang
([`AccessMenuSeeder.cs:151-160`](../../../../Seeders/AccessMenuSeeder.cs#L151-L160)):

```csharp
action.IsActive = true;
action.IsDelete = false;
```

**Langkah 3 — kesimpulannya.** Identitas yang kembali dideklarasikan source akan **diaktifkan
kembali**, bukan dibuat ulang. Policy lama yang menunjuknya ikut hidup kembali bersamanya.

> `AccessMenuSeeder` memang tidak pernah membuat `SysAccessPolicy` — invarian itu tetap utuh dan
> tidak dilanggar. Yang terjadi di sini berbeda: baris policy-nya **sudah ada sejak dulu**, dan yang
> berubah hanyalah status identitas yang ditunjuknya.

---

## 3. Kenapa ini harus diputuskan, bukan dibiarkan

Dibandingkan keadaan sekarang, hasil akhirnya **jauh lebih sempit**: dari "setiap pengguna yang
punya login" menjadi "dua pasangan Departemen × Posisi". Secara keamanan ini perbaikan besar.

Yang tetap tidak boleh didiamkan adalah **cara** kedua pasangan itu memperolehnya: tanpa persetujuan,
tanpa jejak keputusan, dan tanpa muncul di layar mana pun sebagai tindakan pemberian hak. Audit
orphan [`12-authorization-orphan-audit.md`](12-authorization-orphan-audit.md) dijalankan justru untuk
memastikan tidak ada hak yang berpindah diam-diam; membiarkan hal ini terjadi akan membatalkan
tujuan audit itu sendiri.

Perlu dicatat juga bahwa kedua pasangan ini memegang hak tersebut pada masa ketika
`WorkSchedule.Update`/`Delete` **tidak pernah benar-benar ditegakkan**. Jadi memegang baris itu dulu
bukan bukti bahwa mereka memang berwenang — hanya bukti bahwa seseorang pernah mencentangnya pada
layar Akses Role saat identitasnya masih tampil.

---

## 4. Yang wajib dilakukan sebelum seeder dijalankan

| No | Tindakan | Penanggung jawab |
|---:|---|---|
| 1 | Jawab: apakah Finance × Manajer Finance **seharusnya** boleh mengubah dan menghapus jadwal kerja? | Pemilik modul HR |
| 2 | Jawab pertanyaan yang sama untuk Human Resource × Manajer HR | Pemilik modul HR |
| 3 | Bila jawabannya **ya** — tidak ada tindakan teknis; catat keputusannya supaya reaktivasinya menjadi pemberian hak yang disengaja | Pemilik modul HR |
| 4 | Bila jawabannya **tidak** — nonaktifkan keempat baris `SysAccessPolicy` itu **sebelum** source hasil `BE-SEC-012` mencapai lingkungan yang menjalankan seeder | Wewenang database terpisah |
| 5 | Bila jawabannya **belum diputuskan** — jangan jalankan seeder dengan source ini di lingkungan mana pun yang datanya berarti | Pemilik sistem |

Urutan pada butir 4 tidak dapat dibalik. Sesudah seeder berjalan, haknya sudah hidup dan
pencabutannya menjadi tindakan mencabut hak yang sedang dipakai, bukan mencegah hak yang belum ada.

---

## 5. Batas catatan ini

| Hal | Status |
|---|---|
| `SysAccessPolicy` | **Tidak diubah.** Tidak ada satu pun baris disentuh |
| `AccessMenuSeeder` | **Tidak dijalankan** |
| Database | **Tidak dihubungi.** Angka policy dikutip dari ekspor DEV yang disediakan pemilik |
| Aplikasi | **Tidak dinyalakan** |
| Cakupan | Hanya `WorkSchedule.Update` dan `WorkSchedule.Delete`. `Shift`, `ShiftGroup`, `ShiftPattern`, dan `WorkCalendar` **tidak** punya policy orphan pada bukti DEV, sehingga perbaikannya tidak menghidupkan hak siapa pun |
