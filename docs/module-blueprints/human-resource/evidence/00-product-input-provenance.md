# Human Resource — Provenance Masukan Produk

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Status berkas ini | Catatan provenance, bukan sumber kewenangan |
| Dibuat | 2026-08-27 |
| Menutup | `HRD-Q-16` |

---

## 1. Masalah yang diselesaikan berkas ini

Blueprint HR merujuk sebuah PRD yang dibuat di luar alur skill. Impact scan 27 Agustus 2026
menemukan berkas itu **hanya ada di branch `AndryZain`** dan tidak ada di baseline canonical
`origin/QuilvianIntegrationBackend`. Akibatnya, begitu blueprint berpindah ke baseline canonical,
rujukan itu akan menunjuk berkas yang tidak ada.

Penyelesaiannya: berkas itu disalin ke dalam folder blueprint sebagai **snapshot**, lengkap
dengan sidik jari dan asal-usulnya. Blueprint menjadi mandiri dan rujukannya tidak lagi putus,
ke mana pun folder ini dibawa.

## 2. Berkas yang disnapshot

| Field | Value |
| --- | --- |
| Nama asli | `docs/Modul-RS/PRD_to_MVP_HRD_Quilvian_Target_100.md` |
| Lokasi snapshot | [`01-product-input-prd-hrd.snapshot.md`](./01-product-input-prd-hrd.snapshot.md) |
| SHA-256 | `3364e50060b95cd7c9a540d9dc943e59e178a485b167a6d585c80f4629e79169` |
| Jumlah baris | 1.650 |
| Commit asal | `ecdc135444f0110482c9702212bcea30043983c8` — branch `AndryZain`, 27 Agustus 2026, oleh `devbenari` |
| Ada di baseline canonical? | **Tidak.** Commit `ecdc135` adalah satu-satunya tempat berkas ini hidup |
| Isi snapshot | **Identik** dengan aslinya. Tidak ada satu karakter pun yang diubah |

Snapshot sengaja dibiarkan apa adanya. Tujuh konflik antara isi PRD dan kenyataan source sudah
tercatat terpisah pada [`../00-interview-decisions.md`](../00-interview-decisions.md) bagian 5,
beserta usulan perbaikannya. Memperbaiki snapshot berarti menghapus jejak keadaan aslinya.

## 3. Kewenangan berkas ini

**Menyalin berkas ke sini tidak mengubah kewenangannya sedikit pun.**

PRD tersebut tetap berstatus **masukan produk historis**, sesuai `HRD-DEC-002`. Ia bukan PRD
blueprint dan bukan sumber kebenaran.

Susunan kewenangan yang berlaku:

| Artefak | Kewenangannya |
| --- | --- |
| [`../00-interview-decisions.md`](../00-interview-decisions.md) | Keputusan manusia |
| [`../01-existing-capability-map.md`](../01-existing-capability-map.md) | Bukti source apa adanya, hasil audit |
| `../02-backend-architecture.md`, `../03-frontend-architecture.md`, `../contracts/**`, `../data/data-dictionary.md`, `../flowcharts/**` | Desain target |
| `../04-prd-to-mvp.md` | **PRD resmi modul ini**, ditulis paling akhir |
| Snapshot pada folder ini | Masukan produk historis. Tidak mengikat apa pun |

Bila isi snapshot bertentangan dengan salah satu artefak di atas, **artefak di atas yang
berlaku**. Snapshot hanya menjelaskan dari mana pemikiran awal modul ini berasal.

## 4. Cara memverifikasi snapshot

```bash
sha256sum docs/module-blueprints/human-resource/evidence/01-product-input-prd-hrd.snapshot.md
# harus menghasilkan 3364e50060b95cd7c9a540d9dc943e59e178a485b167a6d585c80f4629e79169
```

Bila hasilnya berbeda, snapshot sudah diubah dan tidak lagi dapat dipakai sebagai catatan
historis yang dapat dipercaya.

## 5. Yang tidak boleh dilakukan terhadap berkas ini

1. Jangan memperbaiki isinya agar cocok dengan keputusan blueprint yang lebih baru.
2. Jangan mengutipnya sebagai dasar keputusan. Kutip decision log atau capability map.
3. Jangan memperlakukan angka di dalamnya sebagai fakta — termasuk klaim cakupan 83% yang sudah
   dinyatakan tidak dapat direproduksi lewat `HRD-CONF-03`.
4. Jangan menghapusnya. Ia satu-satunya jejak keadaan awal pemikiran modul ini.
