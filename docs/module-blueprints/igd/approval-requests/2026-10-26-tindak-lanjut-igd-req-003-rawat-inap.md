# Tindak Lanjut `IGD-REQ-003` ke Pemilik Rawat Inap — Dua Butir yang Masih Menahan `BE-IGD-077`

| Field | Value |
|---|---|
| `dokumen` | Tindak lanjut atas [`IGD-REQ-003`](2026-10-18-permintaan-pemilik-rawat-inap-transfer-igd.md) — **bukan** permintaan baru |
| `tanggal` | 2026-10-26 |
| `pengaju` | Rizki Gunawan — Product/Domain Owner IGD (`IGD-DEC-089`) |
| `penerima` | **Muhammad Hamzah** — PIC Admisi Rawat Inap & InPatient Management |
| `rujukan` | `IGD-REQ-003` butir 1 dan 2; `IGD-OQ-132`; `IGD-DEC-308`, `IGD-DEC-309` |
| `status` | **`menunggu`** — 8 hari sejak 18 Oktober 2026 |
| `menahan` | `BE-IGD-077` acceptance 3–4 (5 dari 7 terpenuhi) |

---

## 1. Kabar baik lebih dulu

**`IGD-REQ-004` sudah Mas Hamzah setujui** pada 19 Oktober 2026 (rujukan `RWI-DEC-338`…`341`) —
urutan Antrean Transfer CITO → member prioritas → waktu beserta lencana tier. Terima kasih; itu
tidak lagi menahan apa pun.

Yang tersisa dari `IGD-REQ-003` tinggal **dua butir**, dan keduanya sudah separuh jalan: **sisi IGD
sudah selesai, tinggal satu pemanggilan dari sisi Rawat Inap.**

## 2. Keadaan dua butir itu, diperiksa terhadap source hari ini

Diperiksa pada `rizkiG` sesudah merge integration 26 Oktober 2026 (`56a19904`, memuat seluruh kerja
Rawat Inap sampai 25 Oktober).

| Butir | Yang IGD sediakan | Keadaan di sisi Rawat Inap |
|---:|---|---|
| **1** — panggil evaluasi penutupan IGD sesudah *admit* | `EvaluasiPenutupanKunjunganSusulanAsync` **sudah ada**, `EmergencyDispositionService.AdmissionRequest.cs:1547` | **Nol pemanggil.** `InpAdmissionTransferService` belum memanggilnya |
| **2** — endpoint dan tombol *Tolak* beralasan di Antrean Transfer | `BatalkanFormPermintaanOlehPetugasAdmisiAsync` **sudah ada**, `EmergencyDispositionService.AdmissionRequest.cs:1474` | **Nol pemanggil.** Belum ada endpoint `POST /admission-transfers/{id}/reject` |

Jadi tidak ada yang perlu dirancang lagi dan tidak ada kontrak yang perlu disepakati ulang. Yang
dibutuhkan harfiah **satu baris pemanggilan** untuk butir 1, dan **satu endpoint tipis** untuk
butir 2.

### Butir 1 — bentuk pemanggilannya

Tepat sesudah `EmgDisposition` menjadi `Executed`, dalam transaksi yang sama dan **sebelum**
`SaveChangesAsync`, di dalam blok `if (request.SourceType == "Emergency")` pada
`InpAdmissionTransferService.ExecuteTransferAdmissionCoreAsync`:

```csharp
await _emergencyDispositionService.EvaluasiPenutupanKunjunganSusulanAsync(
    disposition.EmergencyVisitId, actorUserId, DateTime.UtcNow, cancellationToken);
```

### Butir 2 — bentuk endpoint-nya

`POST /admission-transfers/{id}/reject`, alasan **wajib**, meneruskan ke
`BatalkanFormPermintaanOlehPetugasAdmisiAsync(dispositionId, reason, officerUserId, ct)`. Hanya
**form permintaan** yang batal; tindak lanjut IGD tetap *Dikonfirmasi*, sesuai `IGD-DEC-309`.

## 3. Akibat yang berjalan hari ini

Tanpa butir 1, pasien yang sudah selesai di-*admit* **tetap terbuka di IGD** berlencana *"Menunggu
keputusan"* — bertentangan dengan roadmap Rawat Inap sendiri: *"Pasien tidak boleh berstatus
'Menunggu Kamar' lagi di IGD setelah admisi selesai."*

Tanpa butir 2, petugas admisi yang menerima permintaan rawat inap keliru **tidak punya cara menolaknya**.
Satu-satunya jalan keluar hari ini adalah membiarkannya menggantung di antrean.

## 4. Dua butir lain dari `IGD-REQ-003` — mohon dikonfirmasi saja

| Butir | Permintaan | Yang kami butuhkan |
|---:|---|---|
| **3** | `BedManagement:UnitAssignments` memuat petugas admisi di tiap lingkungan, termasuk operasi `InpatientEpisode:Create` | Konfirmasi apakah sudah terpasang di dev, staging, dan produksi. Pada uji B6 dan B7 kami memakai jalan memutar *admit* oleh SuperAdmin (`IGD-FACT-134`) |
| **4** | Nilai master kelas **"KELAS II"** — semula `isForInpatient = false` sehingga *admit* ditolak `422`; penguji mengubahnya menjadi `true` atas izin pemilik IGD (`IGD-DEC-322`) | Konfirmasi nilai yang benar, supaya perubahan uji itu tidak diam-diam menjadi keadaan tetap |

## 5. Yang kami minta sekarang

Bukan pekerjaan besar — **satu jadwal**. Mohon beri tahu kapan butir 1 dan 2 dapat dikerjakan sisi
Rawat Inap, supaya kami dapat menjadwalkan uji bersama Putaran B7 bagian 2 dan menutup `BE-IGD-077`.

Bila butir 2 dinilai bukan prioritas sekarang, mohon dinyatakan begitu — kami akan memisahkan
acceptance 4 `BE-IGD-077` sebagai celah yang dinyatakan, alih-alih menahan task itu tanpa batas
waktu.
