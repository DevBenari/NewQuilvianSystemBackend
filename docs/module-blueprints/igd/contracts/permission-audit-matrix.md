# Permission dan Audit Matrix — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `4` |
| `contract_version` | `0.2.0-draft` |
| Commit diaudit | backend `e5331a0` |
| Keputusan yang mengikat | `IGD-DEC-026`, `IGD-DEC-050` |

String `[AccessPermission(...)]` ditulis apa adanya agar implementer menyalin, bukan
menerjemahkan.

Konvensi logging project: Create, Update, perubahan status workflow, dan Delete dicatat;
`GET` tidak dicatat agar volume log tidak berlebihan.

---

## 1. Daftar permission per endpoint

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `GET /emergency-visits` | `EmergencyVisit` | `Read` | `[AccessPermission("EmergencyVisit", "Read")]` | Tidak |
| `POST /emergency-visits` | `EmergencyVisit` | `Create` | `[AccessPermission("EmergencyVisit", "Create")]` | Ya |
| `PUT /emergency-visits/{id}` | `EmergencyVisit` | `Update` | `[AccessPermission("EmergencyVisit", "Update")]` | Ya |
| `PATCH /emergency-visits/{id}/registration-status` | `EmergencyVisit` | `Update` | `[AccessPermission("EmergencyVisit", "Update")]` | Ya |
| `PATCH /emergency-visits/{id}/visit-status` | `EmergencyVisit` | `Update` | `[AccessPermission("EmergencyVisit", "Update")]` | Ya |
| `PATCH /emergency-visits/{id}/complete` | `EmergencyVisit` | `Update` | `[AccessPermission("EmergencyVisit", "Update")]` | Ya |
| `DELETE /emergency-visits/{id}` | `EmergencyVisit` | `Delete` | `[AccessPermission("EmergencyVisit", "Delete")]` | Ya |
| `GET /emergency-triages` | `EmergencyTriage` | `Read` | `[AccessPermission("EmergencyTriage", "Read")]` | Tidak |
| `GET /emergency-triages/sla-breaches` | `EmergencyTriage` | `Read` | `[AccessPermission("EmergencyTriage", "Read")]` | Tidak |
| `POST /emergency-triages` | `EmergencyTriage` | `Create` | `[AccessPermission("EmergencyTriage", "Create")]` | Ya |
| `POST /emergency-triages/{id}/retriage` | `EmergencyTriage` | `Update` | `[AccessPermission("EmergencyTriage", "Update")]` | Ya |
| `PUT /emergency-triages/{id}` | `EmergencyTriage` | `Update` | `[AccessPermission("EmergencyTriage", "Update")]` | Ya |
| `PATCH /emergency-triages/{id}/triage-status` | `EmergencyTriage` | `Update` | `[AccessPermission("EmergencyTriage", "Update")]` | Ya |
| `DELETE /emergency-triages/{id}` | `EmergencyTriage` | `Delete` | `[AccessPermission("EmergencyTriage", "Delete")]` | Ya |

Pola yang sama berlaku untuk tujuh resource lainnya: `EmergencyTriageDetail`,
`EmergencyResuscitation`, `EmergencyObservation`, `EmergencyObservationDetail`,
`EmergencyProcedureDetail`, `EmergencyDisposition`, dan `EmergencyTransfer` — masing-masing
dengan action `Read`, `Create`, `Update`, dan `Delete`.

Total 52 endpoint pada 9 resource.

---

## 2. Cara kerja pemeriksaan akses saat ini

Berdasarkan `Services/Security/AccessPermissionService.cs` pada commit `e5331a0`:

1. Pengguna harus sudah masuk, jika tidak dikembalikan 401.
2. Pemegang role `SuperAdmin` atau `UserType == 1` **melewati seluruh pemeriksaan**.
3. Selain itu, sistem mencari `SysActionAccess` yang cocok dengan nama controller dan nama
   action, lalu memeriksa `SysAccessPolicy` yang cocok dengan kombinasi Department dan
   Position pengguna, dengan masa berlaku penugasan diperhitungkan.
4. Hasilnya boolean, tanpa konteks resource.

Kemampuan yang **tersedia**: kebijakan berbasis Department dan Position, serta masa berlaku
penugasan organisasi.

Kemampuan yang **belum ada**: scope resource dan unit pelayanan, pemeriksaan kompetensi
klinis, dan mekanisme akses darurat.

---

## 3. Kebutuhan perubahan authorization

Ketiganya berasal dari keputusan IGD tetapi diterapkan di luar modul IGD.

| No | Kebutuhan | Sumber | Status |
| ---: | --- | --- | --- |
| 1 | Pemisahan kewenangan SuperAdmin: tetap penuh untuk endpoint bertanda `IsSystemOnly`, tunduk policy untuk endpoint klinis dan bisnis | `IGD-DEC-050` | **Conflict** dengan kode saat ini |
| 2 | Scope resource dan unit pelayanan pada pemeriksaan akses | `IGD-DEC-026` | **Missing** |
| 3 | Break-glass akses darurat yang tercatat, berbatas waktu, dan dapat ditinjau | `IGD-DEC-050` | **Missing** |

### Rincian kebutuhan 1

`AccessPermissionService.IsSuperAdminUser` saat ini mengembalikan akses penuh tanpa syarat.
Padahal endpoint bertanda `IsSystemOnly` memang sengaja dikecualikan dari pencarian policy,
sehingga bypass tersebut punya fungsi teknis yang sah dan sedang dipakai.

Target desain: bypass dipertahankan **hanya** untuk endpoint `IsSystemOnly`. Untuk endpoint
klinis dan bisnis, SuperAdmin mengikuti policy seperti pengguna lain.

### Rincian kebutuhan 3

Akses darurat klinis tidak boleh dihilangkan begitu saja saat kebutuhan 1 diterapkan. Break-glass
wajib memenuhi: alasan tercatat, berbatas waktu, memicu audit, dan dapat ditinjau setelahnya.
Mekanisme ini belum ada di kode mana pun.

---

## 4. Kewenangan per peran

Mengikuti `IGD-DEC-026`: kewenangan berbasis capability dan konteks, divalidasi di backend,
dengan pemisahan tugas.

| Peran | Kewenangan utama | Batasan |
| --- | --- | --- |
| Petugas pendaftaran | Registrasi, identitas, kunjungan | Tidak berwenang pada keputusan klinis |
| Perawat IGD | Triage, retriage, observasi, catatan berkala, pengajuan transfer | Tidak menetapkan disposition |
| Dokter IGD | Keputusan klinis, tindakan, disposition, penyelesaian kunjungan | — |
| Petugas unit tujuan | Menerima, menolak, dan menyelesaikan transfer | Terbatas pada unit tujuannya |
| Kepala jaga | Pembatalan dengan alasan | Tidak menggantikan kewenangan klinis dokter |
| Billing dan Finance | Proses keuangan | Tidak memblokir penyelesaian klinis |
| Administrator teknis | Endpoint `IsSystemOnly` | **Tidak** otomatis memiliki kewenangan klinis atau bisnis |

Baris terakhir adalah inti `IGD-DEC-050`.

---

## 5. Audit

| Peristiwa | Yang dicatat | Yang **tidak** boleh dicatat |
| --- | --- | --- |
| Pembuatan dan perubahan data | `EntityId`, controller, action, status | Keluhan, diagnosis, ringkasan klinis |
| Perubahan status | Status lama, status baru, pelaku, waktu | Isi catatan klinis |
| Akses ditolak | Controller, action, identitas pengguna | — |
| Break-glass | Alasan, durasi, pelaku, sumber daya yang diakses | Isi rekam medis yang dibaca |

Kolom bertanda sensitif pada [data-dictionary.md](../erd/data-dictionary.md) dilarang masuk
custom logger tanpa kecuali.

Seluruh perubahan tersimpan pada kolom audit `IdentityModel`, dan penghapusan berupa
penandaan sehingga riwayat tetap dapat ditelusuri.
